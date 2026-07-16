using Microsoft.Maui.Controls;
using FrotaGo.Mobile.Models;
using FrotaGo.Mobile.Services;

namespace FrotaGo.Mobile.Views;

public partial class LessonDetailPage : ContentPage
{
    private readonly Lesson _lesson;
    private TrackingService? _trackingService;

    public LessonDetailPage(Lesson lesson)
    {
        InitializeComponent();
        _lesson = lesson;
        TitleLabel.Text = "Aula prática";
        StudentLabel.Text = $"Aluno: {_lesson.StudentName}";
        VehicleLabel.Text = $"Veículo: {_lesson.Vehicle}";
        StatusLabel.Text = $"Estado: {_lesson.Status}";
    }

    private async void OnStartLessonClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Confirmar", "Confirma iniciar a aula e ativar GPS?", "Sim", "Não");
        if (!confirm) return;

        var token = await AuthService.GetTokenAsync();
        var api = new ApiService(MobileConfig.BaseUrl, token);

        var ok = await api.StartLessonAsync(_lesson.Id);
        if (!ok)
        {
            await DisplayAlert("Erro", "Não foi possível iniciar a aula no servidor.", "OK");
            return;
        }

        _trackingService = new TrackingService(api);
        // Usar os IDs reais vindos do modelo da aula
        var started = await _trackingService.StartAsync(_lesson.Id, _lesson.VehicleId, _lesson.InstructorId);
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

        // Usar sempre MobileConfig.BaseUrl — mesma config da app inteira
        var token = await AuthService.GetTokenAsync();
        var api = new ApiService(MobileConfig.BaseUrl, token);

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
