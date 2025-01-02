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
			string newSenderId = "GuestId";
            var currentUser = await userManager.GetUserAsync(User);

			if (currentUser != null)
			{
				newSenderId = currentUser.Id;
			}
                var users = context.Users
				.Where(u => u.Id != newSenderId && u.Id != "GuestId")
				.ToList(); // Hämta alla användare förutom den inloggade
			ViewBag.Users = users; // Skicka användarna till vyn
            Message message = new Message { SenderId = newSenderId };
			return View(message);
		}
		[HttpPost]
		public async Task<ActionResult> SendMessage(string content, string receiver)
		{
			string newSenderId = "GuestId";
			var currentUser = await userManager.GetUserAsync(User);
			if (currentUser != null)
			{
				newSenderId = currentUser.Id;
			}
			var receiverUser = await userManager.FindByNameAsync(receiver);
			
				//Skapar en ny lista för att skicka meddelandet
				var newMessage = new Message
				{
					SenderId = newSenderId,
					ReceiverId = receiverUser.Id,
					Date = DateTime.UtcNow,
					Content = content,
					IsRead = false
				};
				context.Message.Add(newMessage);
				await context.SaveChangesAsync();
			//Lägg till funktionalitet att skicka felmeddelande om man skickar till sig själv
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

            //Hämta alla nuvarande olästa meddelanden
            var unreadMessages = context.Message
                .Where(m => m.ReceiverId == currentUser.Id && m.IsRead == false)
                .ToListAsync();

            //Sätt alla meddelanden till lästa
            foreach (var message in unreadMessages.Result)
            {
                message.IsRead = true;
            }

			await context.SaveChangesAsync();

            //Skapar en ny vy
            var viewModel = new MessageViewModel
			{
				ProfileName = currentUser.UserName,
				Messages = await context.Message
					.Where(m => m.ReceiverId == currentUser.Id)
					.OrderByDescending(m => m.Date)
					.ToListAsync()
			};

			return View(viewModel);
		}
	}

}
