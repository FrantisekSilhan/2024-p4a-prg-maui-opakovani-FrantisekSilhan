using UserDataMAUI.Data;
using UserDataMAUI.Data.Models;
using UserDataMAUI.ViewModels;

namespace UserDataMAUI {
    public partial class AppShell : Shell {
        private readonly Database _database;
        public MainViewModel MVM { get; set; }
        public AppShell() {
            _database = Database.Instance;
            MVM = new MainViewModel(UserService.Instance(_database));
            InitializeComponent();
        }
    }
}
