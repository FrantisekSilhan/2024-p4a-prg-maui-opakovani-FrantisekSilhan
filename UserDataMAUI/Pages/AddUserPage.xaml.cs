namespace UserDataMAUI.Pages;

public partial class AddUserPage : ContentPage
{
	public AddUserPage()
	{
		InitializeComponent();
        BindingContext = (Application.Current!.MainPage! as AppShell)!.MVM;
    }
}