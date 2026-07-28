using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrotaGo.Mobile.Core.Interfaces;

namespace FrotaGo.Mobile.Core.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public Func<Task>? OnLoginSuccess { get; set; }

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Entrar no FrotaGo";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, preencha o e-mail e a palavra-passe.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var result = await _authService.LoginAsync(Email, Password);
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                if (OnLoginSuccess != null)
                {
                    await OnLoginSuccess();
                }
            }
            else
            {
                ErrorMessage = "Credenciais inválidas. Verifique os seus dados.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao efetuar login: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
