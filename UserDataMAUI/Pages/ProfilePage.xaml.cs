namespace UserDataMAUI.Pages;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
        BindingContext = (Application.Current!.MainPage! as AppShell)!.MVM;
    }
}