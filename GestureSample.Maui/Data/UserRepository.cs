using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    public class UserRepository : BaseRepository<User>
    {
        private static readonly List<User> _users = new();

        public UserRepository(SQLiteAsyncConnection database) : base(database) { }
        public Task<List<User>> GetUsersAsync()
        {
            // Return a copy to avoid direct list manipulation
            var listCopy = _users.ToList();
            return Task.FromResult(listCopy);
        }

        public Task<User> GetUserAsync(Guid id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task AddUserAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.Name = user.Name;
                existing.AvatarUri = user.AvatarUri;
            }
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(User user)
        {
            _users.Remove(user);
            return Task.CompletedTask;
        }
    }
}
