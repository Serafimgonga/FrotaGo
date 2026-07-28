using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrotaGo.Mobile.Core.Interfaces;

namespace FrotaGo.Mobile.Core.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ILessonService _lessonService;

    [ObservableProperty]
    private string _instructorName = string.Empty;

    [ObservableProperty]
    private string _schoolName = string.Empty;

    [ObservableProperty]
    private int _todayLessonsCount;

    public DashboardViewModel(IAuthService authService, ILessonService lessonService)
    {
        _authService = authService;
        _lessonService = lessonService;
        Title = "Painel do Instrutor";
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var profile = _authService.CurrentProfile;
            if (profile != null)
            {
                InstructorName = profile.Name;
                SchoolName = profile.SchoolName;

                var lessons = await _lessonService.GetTodayLessonsAsync(profile.InstructorId);
                TodayLessonsCount = lessons.Count;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dados: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
