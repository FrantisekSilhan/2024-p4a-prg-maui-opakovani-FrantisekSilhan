using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserDataMAUI.Data.Models;
using UserDataMAUI.ViewModels;

namespace UserDataMAUI.Data
{
    public class Database
    {
        private static Database? _instance;
        private static readonly object _lock = new object();
        SQLiteAsyncConnection _database;

        public const string DBFilename = "UserDataMAUI.db";
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;
        public static string DatabasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DBFilename);

        public Database()
        {
            Init();
        }
        public static Database Instance
        {
            get
            {
                // Double-check locking for thread safety
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Database();
                        }
                    }
                }
                return _instance;
            }
        }

        async void Init()
        {
            if (_database is not null)
            {
                return;
            }

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await _database.CreateTableAsync<Item>();
            await _database.CreateTableAsync<User>();

            string adminSalt = UserService.GenerateSalt();
            string adminPassword = UserService.HashPassword("admin", adminSalt);
            User? adminUser = await GetUserAsync("admin");
            if (adminUser == null)
            {
                await SaveUserAsync(new User { Username = "admin", Password = adminPassword, Salt = adminSalt });
                await SaveItemAsync(new Item { Title = "First admin Item", Description = "This is an item description.", Author = 1 });
            }
        }

        public async Task<List<Item>> GetItemsAsync()
        {
            Init();
            return await _database.Table<Item>().ToListAsync();
        }

        public async Task<int> SaveItemAsync(Item item)
        {
            Init();
            if (item.ID != 0)
                return await _database.UpdateAsync(item);

            return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync(Item item)
        {
            Init();
            return await _database.DeleteAsync(item);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            Init();
            return await _database.Table<User>().ToListAsync();
        }

        public async Task<int> SaveUserAsync(User user)
        {
            Init();
            if (user.ID != 0)
                return await _database.UpdateAsync(user);

            return await _database.InsertAsync(user);
        }

        public async Task<int> DeleteUserAsync(User user)
        {
            Init();
            return await _database.DeleteAsync(user);
        }

        public List<Item> GetUserItems(string username)
        {
            Init();
            Task<User> user = _database.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();

            user.Wait();
            if (user.Result is null)
            {
                return new List<Item>();
            }
            var item = _database.Table<Item>().Where(i => i.Author == user.Result.ID).ToListAsync();

            item.Wait();

            return item.Result;
        }

        public async Task<User?> GetUserAsync(string username)
        {
            Init();
            return await _database.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<bool> usernameExists(string username)
        {
            Init();
            return await _database.Table<User>().Where(u => u.Username == username).CountAsync() > 0;
        }
    }
}
