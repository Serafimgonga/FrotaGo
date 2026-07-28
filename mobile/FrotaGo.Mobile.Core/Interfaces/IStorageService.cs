namespace FrotaGo.Mobile.Core.Interfaces;

public interface IStorageService
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
    Task SaveStringAsync(string key, string value);
    Task<string?> GetStringAsync(string key);
    Task RemoveKeyAsync(string key);
}
