using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private UserContext context;
        private UserManager<User> userManager;

        public MessageController(ILogger<MessageController> logger, UserContext service, UserManager<User> userManagerr)
        {
            _logger = logger;
            userManager = userManagerr;
            context = service;
        }

        [HttpGet]
        public async Task<ActionResult> SendMessageAsync()
        {
            try
            {
                // Hämta den inloggade användaren
                var currentUser = await userManager.GetUserAsync(User);
                var currentUserId = currentUser?.Id ?? string.Empty;

                // Hämta endast aktiva användare och exkludera den inloggade användaren
                var users = context.Users
                    .Where(u => u.IsEnabled) // Endast aktiva konton
                    .Where(u => u.Id != currentUserId) // Exkludera den inloggade användaren
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    }).ToList();

                if (!users.Any())
                {
                    TempData["Error"] = "Det finns inga användare att skicka meddelanden till.";
                    return RedirectToAction("Index", "Home");
                }

                // Skicka användarlistan till vyn
                ViewBag.Users = new SelectList(users, "Value", "Text");
                return View(new Message());
            }
            catch (Exception ex)
            {
                // Logga felet och visa ett felmeddelande
                _logger.LogError($"Ett fel inträffade: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade när användarlistan hämtades.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(Message model)
        {
            try
            {
                // Hämta inloggad användare (kan vara null för gäster)
                var currentUser = await userManager.GetUserAsync(User);

                if (ModelState.IsValid)
                {
                    string senderUsername;

                    // Kontrollera avsändare (inloggad eller gäst)
                    if (currentUser != null)
                    {
                        senderUsername = model.SenderId;
                    }
                    else if (!string.IsNullOrEmpty(model.SenderId))
                    {
                        senderUsername = model.SenderId + " (Gäst)";
                    }
                    else
                    {
                        TempData["Error"] = "Avsändare måste anges.";
                        return RedirectToAction("SendMessageAsync");
                    }

                    // Kontrollera om mottagare är angiven
                    if (string.IsNullOrEmpty(model.ReceiverId))
                    {
                        TempData["Error"] = "Du måste välja en mottagare.";
                        return RedirectToAction("SendMessageAsync");
                    }

                    // Kontrollera om mottagaren är aktiv
                    var receiver = await context.Users.FirstOrDefaultAsync(u => u.Id == model.ReceiverId && u.IsEnabled);
                    if (receiver == null)
                    {
                        TempData["Error"] = "Mottagaren är inaktiverad och kan inte ta emot meddelanden.";
                        return RedirectToAction("SendMessageAsync");
                    }

                    // Kontrollera om innehåll är angivet
                    if (string.IsNullOrEmpty(model.Content))
                    {
                        TempData["Error"] = "Du har inte fyllt i meddelanderutan.";
                        return RedirectToAction("SendMessageAsync");
                    }

                    // Skapa nytt meddelande
                    var newMessage = new Message
                    {
                        SenderId = senderUsername,
                        ReceiverId = model.ReceiverId,
                        Date = DateTime.UtcNow,
                        Content = model.Content,
                        IsRead = false
                    };

                    // Spara meddelandet i databasen
                    context.Message.Add(newMessage);
                    await context.SaveChangesAsync();

                    // Skicka användaren till rätt sida baserat på deras inloggningsstatus
                    if (currentUser != null)
                    {
                        return RedirectToAction("Message", "Message");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    // Logga valideringsfel för debugging
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            _logger.LogError($"Property: {modelState.Key}, Error: {error.ErrorMessage}");
                        }
                    }

                    // Skicka tillbaka användarlistan till vyn
                    var currentUserId = currentUser?.Id ?? string.Empty;
                    var users = context.Users
                        .Where(u => string.IsNullOrEmpty(currentUserId) || u.Id != currentUserId)
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id,
                            Text = u.UserName
                        }).ToList();

                    ViewBag.Users = new SelectList(users, "Value", "Text");
                    return View(model); // Skicka tillbaka samma modell med valideringsfel
                }
            }
            catch (Exception ex)
            {
                // Logga fel och visa ett felmeddelande
                _logger.LogError($"Ett fel inträffade vid sparandet av meddelandet: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade vid sparandet av meddelandet.";
                return RedirectToAction("SendMessageAsync");
            }
        }

        public async Task<ActionResult> Message()
        {
            try
            {
                // Hämtar den inloggade användaren
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Skapa en ny vy med meddelanden sorterade efter datum
                var viewModel = new MessageViewModel
                {
                    Messages = await context.Message
                        .Where(m => m.ReceiverId == currentUser.Id)
                        .OrderByDescending(m => m.Date)
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Logga fel och visa ett felmeddelande
                _logger.LogError($"Ett fel inträffade vid hämtning av meddelanden: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade vid hämtning av meddelanden.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<ActionResult> MarkAsRead(List<int> selectedMessages)
        {
            try
            {
                // Kontrollera om några meddelanden är valda
                if (selectedMessages == null || selectedMessages.Count == 0)
                {
                    TempData["Message"] = "Inga meddelanden valda";
                    return RedirectToAction("Message");
                }

                // Hämta och uppdatera valda meddelanden
                var messagesToUpdate = await context.Message
                    .Where(m => selectedMessages.Contains(m.Id) && !m.IsRead)
                    .ToListAsync();

                foreach (var message in messagesToUpdate)
                {
                    message.IsRead = true;
                }
                await context.SaveChangesAsync();

                TempData["Message"] = "Valda meddelande har markerats som lästa";
                return RedirectToAction("Message");
            }
            catch (Exception ex)
            {
                // Logga fel och visa ett felmeddelande
                _logger.LogError($"Ett fel inträffade vid uppdatering av meddelanden: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade vid uppdatering av meddelanden.";
                return RedirectToAction("Message");
            }
        }

        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            try
            {
                // Hämta den inloggade användaren
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Hämta meddelandet som ska tas bort
                var message = await context.Message
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == currentUser.Id);

                if (message == null)
                {
                    TempData["Message"] = "Meddelandet kunde inte hittas";
                    return RedirectToAction("Message");
                }

                // Ta bort meddelandet och spara ändringarna
                context.Message.Remove(message);
                await context.SaveChangesAsync();

                TempData["Message"] = "Meddelandet har tagits bort";
                return RedirectToAction("Message");
            }
            catch (Exception ex)
            {
                // Logga fel och visa ett felmeddelande
                _logger.LogError($"Ett fel inträffade vid borttagning av meddelandet: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade vid borttagning av meddelandet.";
                return RedirectToAction("Message");
            }
        }
    }
}