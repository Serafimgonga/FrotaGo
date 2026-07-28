using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Interfaces;

namespace FrotaGo.Mobile.Core.ViewModels;

public partial class TodayLessonsViewModel : BaseViewModel
{
    private readonly ILessonService _lessonService;
    private readonly IAuthService _authService;

    public ObservableCollection<TodayLessonDto> Lessons { get; } = new();

    public TodayLessonsViewModel(ILessonService lessonService, IAuthService authService)
    {
        _lessonService = lessonService;
        _authService = authService;
        Title = "Aulas de Hoje";
    }

    [RelayCommand]
    public async Task LoadLessonsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            Lessons.Clear();

            var profile = _authService.CurrentProfile;
            if (profile != null)
            {
                var list = await _lessonService.GetTodayLessonsAsync(profile.InstructorId);
                foreach (var lesson in list)
                {
                    Lessons.Add(lesson);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar aulas: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
