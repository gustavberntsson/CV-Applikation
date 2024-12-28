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
		public ActionResult SendMessage()
		{
			var users = context.Users.ToList(); // Hämta alla användare
			ViewBag.Users = users; // Skicka användarna till vyn
			Message message = new Message();
			return View(message);
		}
		[HttpPost]
        public async Task<ActionResult> SendMessage(string content, string receiver)
        {
            var currentUser = await userManager.GetUserAsync(User);
			if (currentUser == null)
				return RedirectToAction("Login", "Account");

			var receiverUser = await userManager.FindByNameAsync(receiver);
			//Skicka endast meddelandet om det inte är till sig själv
			if (currentUser.UserName != receiver)
			{
				//Skapar en ny lista för att skicka meddelandet
				var newMessage = new Message
				{
					SenderId = currentUser.Id,
					ReceiverId = receiverUser.Id,
					Date = DateTime.UtcNow,
					Content = content,
					IsRead = false
				};
				context.Message.Add(newMessage);
				await context.SaveChangesAsync();
			}
			//Lägg till funktionalitet att skicka felmeddelande om man skickar till sig själv
				
			return RedirectToAction("Message", "Message");
		}

		public async Task<ActionResult> Message()  
		{
			//Hämtar ut användaren först
			var currentUser = await userManager.GetUserAsync(User);
			//Om något går fel så skickar den till loginruta
			if (currentUser == null)
				return RedirectToAction("Login", "Account");

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
