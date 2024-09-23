using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserDataMAUI.Data;
using UserDataMAUI.Data.Models;
using UserDataMAUI.Pages;

namespace UserDataMAUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly Database _database;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private Item? _selectedItem;
        private string _newUserUsername = string.Empty;
        private string _newUserPassword = string.Empty;
        public string NewUserUsername
        {
            get => _newUserUsername;
            set
            {
                _newUserUsername = value;
                OnPropertyChanged();
            }
        }
        public string NewUserPassword
        {
            get => _newUserPassword;
            set
            {
                _newUserPassword = value;
                OnPropertyChanged();
            }
        }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    ClearErrorMessageAfterDelay();
                }
            }
        }
        private async void ClearErrorMessageAfterDelay()
        {
            await Task.Delay(3000);
            ErrorMessage = string.Empty;
        }
        private List<Item> _items;
        public List<Item> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        public Item? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value)
                {
                    _selectedItem = null;
                    Title = string.Empty;
                    Description = string.Empty;
                } else
                {
                    _selectedItem = value;
                    Title = _selectedItem?.Title ?? string.Empty;
                    Description = _selectedItem?.Description ?? string.Empty;
                }
                OnPropertyChanged();
            }
        }
        public ICommand LogoutCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand CloseItemCommand { get; }
        public ICommand AddNewUserCommand { get; }
        public MainViewModel(UserService userService)
        {
            _database = Database.Instance;
            _userService = userService;

            LoginCommand = new Command(Login);
            LogoutCommand = new Command(Logout);
            AddCommand = new Command(Add);
            DeleteItemCommand = new Command<Item>(DeleteItem);
            ChangePasswordCommand = new Command(ChangePassword);
            RemoveItemCommand = new Command(RemoveItem);
            EditItemCommand = new Command(EditItem);
            CloseItemCommand = new Command(CloseItem);
            AddNewUserCommand = new Command(AddNewUser);

            if (_userService.IsUserLoggedIn)
            {
                Items = _database.GetUserItems(_userService.LoggedInUsername);
            }
            else
            {
                Items = new List<Item>();
            }
        }

        public MainViewModel()
        {
        }

        private async void Logout()
        {
            _userService.SetLogout();
            Items = new List<Item>();
            Username = string.Empty;
            Password = string.Empty;
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        private async void Login()
        {
            var (isValid, user) = await _userService.LoginAsync(Username, Password);

            if (isValid && user is not null)
            {
                _userService.SetLogin(user);
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                return;
            }

            Debug.WriteLine($"Login: {isValid}");
            Debug.WriteLine($"IsUserLoggedIn: {_userService.IsUserLoggedIn}");

            if (_userService.IsUserLoggedIn)
            {
                Items = _database.GetUserItems(_userService.LoggedInUsername);
                Debug.WriteLine($"Items: {Items.Count}");
                Username = user.Username;
            }
            else
            {
                Items = new List<Item>();
            }
            ErrorMessage = string.Empty;
        }

        private async void Add()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Title and Description are required";
                return;
            }

            Item item = new Item
            {
                Title = Title,
                Description = Description,
                Author = _userService.LoggedInUserID
            };

            Title = string.Empty;
            Description = string.Empty;

            await _database.SaveItemAsync(item);

            Items = _database.GetUserItems(_userService.LoggedInUsername);
        }

        private async void DeleteItem(Item item)
        {
            await _database.DeleteItemAsync(item);
            Items = _database.GetUserItems(_userService.LoggedInUsername);
        }

        private async void ChangePassword()
        {
            if (await _userService.ChangePasswordAsync(Password))
            {
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "Password change failed";
            }
            Logout();
        }

        public async void RemoveItem()
        {
            if (SelectedItem is null)
            {
                return;
            }

            await _database.DeleteItemAsync(SelectedItem);
            Items = _database.GetUserItems(_userService.LoggedInUsername);
            SelectedItem = null;
        }

        public async void EditItem()
        {
            if (SelectedItem is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Title and Description are required";
                return;
            }

            SelectedItem.Title = Title;
            SelectedItem.Description = Description;

            await _database.SaveItemAsync(SelectedItem);

            Item item = SelectedItem;
            SelectedItem = null;
            SelectedItem = item;
            Items = _database.GetUserItems(_userService.LoggedInUsername);
        }

        public void CloseItem()
        {
            SelectedItem = null;
        }

        public async void AddNewUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserUsername) || string.IsNullOrWhiteSpace(NewUserPassword))
            {
                ErrorMessage = "Username and password are required";
                return;
            }

            if (!_userService.IsUserLoggedIn)
            {
                ErrorMessage = "Only logged in users can add new users";
                return;
            }

            bool userExists = await _database.usernameExists(NewUserUsername);

            if (userExists)
            {
                ErrorMessage = "Username already taken";
                return;
            }

            User newUser = new User
            {
                Username = NewUserUsername,
                Password = NewUserPassword
            };

            string salt = UserService.GenerateSalt();
            newUser.Salt = salt;
            newUser.Password = UserService.HashPassword(newUser.Password, salt);

            await _database.SaveUserAsync(newUser);
        }

        #region MVVM
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //MVVM
    }
}
