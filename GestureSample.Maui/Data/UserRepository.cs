using GestureSample.Maui.Data.SQLite;
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
        

        public UserRepository() : base() { }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _database.Table<User>().OrderByDescending(u => u.LastLoginTime).ToListAsync();
        }

    }
}
