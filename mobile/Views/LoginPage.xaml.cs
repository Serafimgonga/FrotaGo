using Microsoft.Maui.Controls;
using FrotaGo.Mobile.Services;

namespace FrotaGo.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var email = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Erro", "Preencha email e password", "OK");
            return;
        }

        var auth = new AuthService(MobileConfig.BaseUrl);
        var token = await auth.LoginAsync(email, password);
        if (token == null)
        {
            await DisplayAlert("Falhou", "Não foi possível autenticar. Verifique credenciais ou ligação.", "OK");
            return;
        }

        var api = new ApiService(MobileConfig.BaseUrl, token);
        await Navigation.PushAsync(new LessonListPage(api));
    }
}
