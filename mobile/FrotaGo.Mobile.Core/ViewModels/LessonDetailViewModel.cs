using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Interfaces;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.ViewModels;

public partial class LessonDetailViewModel : BaseViewModel
{
    private readonly ILessonService _lessonService;
    private readonly ITrackingService _trackingService;

    [ObservableProperty]
    private Lesson? _currentLesson;

    [ObservableProperty]
    private int _initialKm;

    [ObservableProperty]
    private int _finalKm;

    [ObservableProperty]
    private string _notes = string.Empty;

    public LessonDetailViewModel(ILessonService lessonService, ITrackingService trackingService)
    {
        _lessonService = lessonService;
        _trackingService = trackingService;
        Title = "Detalhes da Aula";
    }

    [RelayCommand]
    public async Task LoadLessonAsync(Guid lessonId)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            CurrentLesson = await _lessonService.GetLessonDetailsAsync(lessonId);
            if (CurrentLesson != null && CurrentLesson.InitialKm.HasValue)
            {
                InitialKm = CurrentLesson.InitialKm.Value;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar detalhes da aula: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task StartLessonAsync()
    {
        if (CurrentLesson == null || IsBusy) return;

        try
        {
            IsBusy = true;
            var request = new StartLessonRequest
            {
                LessonId = CurrentLesson.Id,
                InitialKm = InitialKm,
                InitialLatitude = -8.8368,
                InitialLongitude = 13.2331
            };

            var success = await _lessonService.StartLessonAsync(request);
            if (success)
            {
                await _trackingService.StartTrackingSessionAsync(
                    instructorId: CurrentLesson.StudentId, // or instructor profile id
                    vehicleId: CurrentLesson.VehicleId,
                    lessonId: CurrentLesson.Id
                );
                await LoadLessonAsync(CurrentLesson.Id);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao iniciar aula: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task FinishLessonAsync()
    {
        if (CurrentLesson == null || IsBusy) return;

        try
        {
            IsBusy = true;
            var request = new FinishLessonRequest
            {
                LessonId = CurrentLesson.Id,
                FinalKm = FinalKm,
                Notes = Notes
            };

            var success = await _lessonService.FinishLessonAsync(request);
            if (success)
            {
                if (_trackingService.ActiveSession != null)
                {
                    await _trackingService.EndTrackingSessionAsync(_trackingService.ActiveSession.SessionId);
                }
                await LoadLessonAsync(CurrentLesson.Id);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao finalizar aula: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
