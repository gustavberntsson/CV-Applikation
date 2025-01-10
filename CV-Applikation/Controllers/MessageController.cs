using CV_Applikation.Migrations;
using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
	public class MessageController : Controller
	{
		private UserContext context;
		private UserManager<User> userManager;
		public MessageController(UserContext service, UserManager<User> userManagerr)
		{
			userManager = userManagerr;
			context = service;

		}
        [HttpGet]
        public async Task<ActionResult> SendMessageAsync()
        {
            // Hämta inloggad användare (kan vara null om det är en gäst)
            var currentUser = await userManager.GetUserAsync(User);

            // Hämta aktuell användar-ID (gäst = tom sträng)
            var currentUserId = currentUser?.Id ?? string.Empty;

            try
            {
                // Hämta tillgängliga användare (exkludera inloggad användare om någon är inloggad)
                var users = context.Users
                    .Where(u => string.IsNullOrEmpty(currentUserId) || u.Id != currentUserId) // Tillåt gäster
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    }).ToList();

                // Kontrollera om det finns några användare att skicka till
                if (!users.Any())
                {
                    TempData["Error"] = "Det finns inga användare att skicka meddelanden till.";
                    return RedirectToAction("Index", "Home");
                }

                // Skicka användarlistan till vyn
                ViewBag.Users = new SelectList(users, "Value", "Text");

                // Returnera vyn med en ny meddelandemodell
                return View(new Message());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade: {ex.Message}");
                TempData["Error"] = "Ett fel inträffade när användarlistan hämtades.";
                return RedirectToAction("Index", "Home");
            }
        }
    

        [HttpPost]
        public async Task<ActionResult> SendMessage(Message model)
        {
            // Hämta inloggad användare (kan vara null för gäster)
            var currentUser = await userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                string senderUsername;

                // Kontrollera avsändare (inloggad eller gäst)
                if (currentUser != null)
                {
                    //senderUsername = currentUser.UserName;
                    senderUsername = model.SenderId;
                }
                else if (!string.IsNullOrEmpty(model.SenderId)) // Hantera gästavsändare
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

                try
                {
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Ett fel inträffade vid sparandet av meddelandet: {ex.Message}");
                    TempData["Error"] = "Ett fel inträffade vid sparandet av meddelandet.";
                    return RedirectToAction("SendMessageAsync");
                }
            }
            else
            {
                // Logga valideringsfel för debugging
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Property: {modelState.Key}, Error: {error.ErrorMessage}");
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
       
        public async Task<ActionResult> Message()
		{
			//Hämtar ut användaren först
			var currentUser = await userManager.GetUserAsync(User);
			//Om något går fel så skickar den till loginruta
			if (currentUser == null)
				return RedirectToAction("Login", "Account");

			await context.SaveChangesAsync();

			//Skapar en ny vy
			var viewModel = new MessageViewModel
			{
				Messages = await context.Message
					.Where(m => m.ReceiverId == currentUser.Id)
					.OrderByDescending(m => m.Date)
					.ToListAsync()
			};

			return View(viewModel);
		}

		public async Task<ActionResult> MarkAsRead(List<int> selectedMessages)
		{
			if (selectedMessages == null || selectedMessages.Count == 0)
			{
				TempData["Message"] = "Inga meddelanden valda";
				return RedirectToAction("Message");
			}

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

		public async Task<ActionResult> DeleteMessage(int messageId)
		{
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var message = await context.Message
			 .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == currentUser.Id);

			// Om något skulle gå fel
            if (message == null)
            {
                TempData["Message"] = "Meddelandet kunde inte hittas";
                return RedirectToAction("Message");
            }

            context.Message.Remove(message);
            await context.SaveChangesAsync();

            TempData["Message"] = "Meddelandet har tagits bort";
            return RedirectToAction("Message");
        }
	}
}