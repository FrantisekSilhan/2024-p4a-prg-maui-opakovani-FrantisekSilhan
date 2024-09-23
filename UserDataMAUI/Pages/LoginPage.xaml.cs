using UserDataMAUI.Data;
using UserDataMAUI.Data.Models;
using UserDataMAUI.ViewModels;

namespace UserDataMAUI.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
	{
        BindingContext = (Application.Current!.MainPage! as AppShell)!.MVM;
        InitializeComponent();
	}
}