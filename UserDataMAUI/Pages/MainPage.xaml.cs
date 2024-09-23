using UserDataMAUI.Data;
using UserDataMAUI.Data.Models;
using UserDataMAUI.ViewModels;

namespace UserDataMAUI.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
        InitializeComponent();
        BindingContext = (Application.Current!.MainPage! as AppShell)!.MVM;
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is null)
            return;

        var selectedItem = e.Item as Item;
        (BindingContext as MainViewModel)!.SelectedItem = selectedItem;
    }
}