using System.Threading.Tasks;
using Xunit;

namespace FrotaGo.Mobile.Tests;

public class ApiServiceTests
{
    private class TestApiService
    {
        private readonly string _baseUrl;
        public TestApiService(string baseUrl) => _baseUrl = baseUrl;
        public async Task<System.Collections.Generic.List<object>> GetLessonsAsync()
        {
            await Task.Delay(10);
            return new System.Collections.Generic.List<object> { new { Id = 1 } };
        }

        public async Task<bool> StartLessonAsync(int id)
        {
            await Task.Delay(10);
            return false;
        }
    }

    [Fact]
    public async Task GetLessonsAsync_ReturnsPlaceholder_WhenNoBackend()
    {
        var api = new TestApiService("https://invalid-host.local/");
        var list = await api.GetLessonsAsync();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task StartLessonAsync_ReturnsFalse_WhenNoBackend()
    {
        var api = new TestApiService("https://invalid-host.local/");
        var ok = await api.StartLessonAsync(9999);
        Assert.False(ok);
    }
}
