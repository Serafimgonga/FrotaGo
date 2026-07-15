using Microsoft.Maui.Controls;
using FrotaGo.Mobile.Models;
using FrotaGo.Mobile.Services;
using Microsoft.Maui.Storage;

namespace FrotaGo.Mobile.Views;

public partial class LessonDetailPage : ContentPage
{
    private readonly Lesson _lesson;

    public LessonDetailPage(Lesson lesson)
    {
        InitializeComponent();
        _lesson = lesson;
        TitleLabel.Text = "Aula prática";
        StudentLabel.Text = $"Aluno: {_lesson.StudentName}";
        VehicleLabel.Text = $"Veículo: {_lesson.Vehicle}";
        StatusLabel.Text = $"Estado: {_lesson.Status}";
    }

    private TrackingService? _trackingService;

    private async void OnStartLessonClicked(object? sender, EventArgs e)
    {
        // confirm vehicle/student already set by selection
        var confirm = await DisplayAlert("Confirmar", "Confirma iniciar a aula e ativar GPS?", "Sim", "Não");
        if (!confirm) return;

        var token = Preferences.Get("frotago_auth_token", string.Empty);
        var api = new ApiService(MobileConfig.BaseUrl, token);

        // Start lesson on backend
        var ok = await api.StartLessonAsync(_lesson.Id);
        if (!ok)
        {
            await DisplayAlert("Erro", "Não foi possível iniciar a aula no servidor.", "OK");
            return;
        }

        // start tracking
        _trackingService = new TrackingService(api);
        var started = await _trackingService.StartAsync(_lesson.Id, vehicleId: 0, instructorId: 0);
        if (!started)
        {
            await DisplayAlert("Erro", "GPS não disponível ou permissões negadas. Ative o GPS e tente novamente.", "OK");
            return;
        }

        StatusLabel.Text = "Estado: InProgress (GPS ativo)";
        await DisplayAlert("Aula iniciada", "Aula em progresso e rastreamento iniciado.", "OK");
    }

    private async void OnStopLessonClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Finalizar", "Confirma finalizar a aula?", "Sim", "Não");
        if (!confirm) return;

        var baseUrl = "https://api.example.com/"; // TODO: configurar
        var token = Preferences.Get("frotago_auth_token", string.Empty);
        var api = new ApiService(baseUrl, token);

        if (_trackingService != null)
        {
            await _trackingService.StopAsync();
        }

        var ok = await api.StopLessonAsync(_lesson.Id);
        if (!ok)
        {
            await DisplayAlert("Erro", "Não foi possível finalizar a aula no servidor.", "OK");
            return;
        }

        StatusLabel.Text = "Estado: Completed";
        await DisplayAlert("Aula finalizada", "Aula finalizada com sucesso.", "OK");
        await Navigation.PopAsync();
    }
}
