using Microsoft.Maui.Controls;

namespace FrotaGo.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new Views.LoginPage());
    }
}
