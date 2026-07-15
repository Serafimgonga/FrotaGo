using Microsoft.Maui.Controls;
using FrotaGo.Mobile.Services;
using FrotaGo.Mobile.Models;

namespace FrotaGo.Mobile.Views;

public partial class LessonListPage : ContentPage
{
    private readonly ApiService _api;

    public LessonListPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var list = await _api.GetLessonsAsync();
        LessonsView.ItemsSource = list;
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is Lesson lesson)
        {
            await Navigation.PushAsync(new LessonDetailPage(lesson));
        }
    }
}
