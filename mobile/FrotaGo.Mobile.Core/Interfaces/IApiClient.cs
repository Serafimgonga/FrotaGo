namespace FrotaGo.Mobile.Core.Interfaces;

public interface IApiClient
{
    Task<TResponse?> GetAsync<TResponse>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
    Task<bool> PostAsync<TRequest>(string endpoint, TRequest request);
}
