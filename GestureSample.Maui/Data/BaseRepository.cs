using GestureSample.Maui.Data.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    public class BaseRepository<T> where T : new()
    {
        protected SQLiteAsyncConnection _database;

        public BaseRepository()
        {
            _database = StateConnection.Instance.Database; 
        }


        public Task<int> SaveAsync(T entity) => _database.InsertAsync(entity);
        public Task<int> UpdateAsync(T entity) => _database.UpdateAsync(entity);
        public Task<int> DeleteAsync(T entity) => _database.DeleteAsync(entity);
        public Task<List<T>> GetAllAsync() => _database.Table<T>().ToListAsync();
        public Task<T> GetByIdAsync(object id) => _database.FindAsync<T>(id);
    }
}
