using CV_Applikation.Migrations;
using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<ActionResult> SendMessageAsync()
		{
			var currentUser = await userManager.GetUserAsync(User);
			var isUserLoggedIn = currentUser != null;
			
			var currentUserId = currentUser?.Id ?? string.Empty;

            var users = context.Users
			.Where(u => u.Id != currentUserId)
			.Where(u => isUserLoggedIn || u.IsPrivate == false)
			.ToList(); // Hämta alla användare förutom den inloggade
			ViewBag.Users = users; // Skicka användarna till vyn
			Message message = new Message {};
			return View(message);
		}
		[HttpPost]
		public async Task<ActionResult> SendMessage(string content, string receiver, string sender)
		{

            if (string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Du har inte fyllt i meddelanderutan.";
                return RedirectToAction("SendMessageAsync");
            }

            var currentUser = await userManager.GetUserAsync(User);
			string senderUsername;

            if (currentUser != null)
            {
                senderUsername = currentUser.UserName;
            }
            else if (!string.IsNullOrEmpty(sender))
            {
                senderUsername = sender + " (Gäst)";
            }
            else
            {
                TempData["Error"] = "Avsändare måste anges";
                return RedirectToAction("SendMessageAsync");
            }

			var receiverUser = await userManager.FindByNameAsync(receiver);

			//Skapar en ny lista för att skicka meddelandet
			var newMessage = new Message
			{
				SenderId = senderUsername,
				ReceiverId = receiverUser.Id,
				Date = DateTime.UtcNow,
				Content = content,
				IsRead = false
			};

			context.Message.Add(newMessage);
			await context.SaveChangesAsync();

			if (currentUser != null)
			{
				return RedirectToAction("Message", "Message");
			}
			else
			{
				return RedirectToAction("Index", "Home");

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