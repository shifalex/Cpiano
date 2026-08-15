using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Handlers
{
    public class CurrentUserSession
    {
        // Once loaded, this property will be available for synchronous access.
        public User ActiveUser { get; private set; }

        public event EventHandler ActiveUserChanged;

        // A reference to your repository (you might inject this via DI instead)
        private UserRepository _userRepo = ServiceHelper.GetService<UserRepository>();

        /// <summary>
        /// Loads the user asynchronously and caches it.
        /// </summary>
        /// <param name="userId">The ID of the user to load.</param>
        public async Task LoadUserAsync(Guid? userId)
        {
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                ActiveUser = null;
                ActiveUserChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Asynchronously load the user data using the repository.
            ActiveUser = await _userRepo.GetByIdAsync(userId);

            ActiveUserChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
