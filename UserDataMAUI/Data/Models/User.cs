using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UserDataMAUI.Data.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public long ID { get; set; }
        [Column("username")]
        public string Username { get; set; } = string.Empty;
        [Column("password")]
        public string Password { get; set; } = string.Empty;
        [Column("salt")]
        public string Salt { get; set; } = string.Empty;
    }

    public class UserService
    {
        private static UserService? _instance;
        private static readonly object _lock = new object();
        private readonly Database _database;
        public static string GenerateSalt(int size = 32)
        {
            var rng = new Random();
            var buffer = new byte[size];
            rng.NextBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public static string HashPassword(string password, string salt)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                string saltedPassword = $"{password}{salt}";
                byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                byte[] hashedBytes = sha512.ComputeHash(saltedPasswordBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashedBytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public void SetLogin(User user)
        {
            Preferences.Set("IsLoggedIn", true);
            Preferences.Set("username", user.Username);
            Preferences.Set("id", user.ID);
        }

        public bool IsUserLoggedIn
        {
            get { return Preferences.Get("IsLoggedIn", false); }
        }

        public string LoggedInUsername
        {
            get { return Preferences.Get("username", string.Empty); }
        }

        public long LoggedInUserID
        {
            get { return (long)Preferences.Get("id", (long)0); }
        }

        public void SetLogout()
        {
            Preferences.Remove("IsLoggedIn");
            Preferences.Remove("username");
        }

        public UserService(Database database)
        {
            _database = database;
        }

        public static UserService Instance(Database database)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UserService(database);
                    }
                }
            }
            return _instance;
        }

        public async Task<bool> ChangePasswordAsync(string newPassword)
        {
            User? user = await _database.GetUserAsync(LoggedInUsername);
            if (user is null)
            {
                return false;
            }

            string newSalt = GenerateSalt();
            string newHashedPassword = HashPassword(newPassword, newSalt);
            user.Salt = newSalt;
            user.Password = newHashedPassword;

            int result = await _database.SaveUserAsync(user);

            return result > 0;
        }

            public async Task<(bool isValid, User?)> LoginAsync(string username, string password)
        {
            User? user = await _database.GetUserAsync(username);
            if (user is null)
            {
                return (false, null);
            }

            string hashedPassword = HashPassword(password, user.Salt);
            if (hashedPassword == user.Password)
            {
                return (true, user);
            }

            return (false, null);
        }
    }
}
