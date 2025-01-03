using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Views.Shared.Components
{
    public class UnreadMessagesViewComponent : ViewComponent
    {
        private readonly UserManager<User> userManager;
        private UserContext context;
        public UnreadMessagesViewComponent(UserManager<User> userManager, UserContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await userManager.GetUserAsync(HttpContext.User);
            int unreadMessageCount = 0;

            if (currentUser != null)
            {
                unreadMessageCount = await context.Message
                    .Where(m => m.ReceiverId == currentUser.Id && !m.IsRead)
                    .CountAsync();
            }

            return View(unreadMessageCount);
        }
    }
}
