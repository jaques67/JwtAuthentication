using BlazorServer.Dto;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BlazorServer.Services;

public class AuthService(
    AccessTokenService accessTokenService,
    NavigationManager nav,
    IHttpClientFactory httpClientFactory)
{
    private readonly AccessTokenService _accessTokenService = accessTokenService;
    private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");
    private readonly NavigationManager _nav = nav;

    public async Task<bool> Login(string email, string password)
    {
        var status = await _client.PostAsJsonAsync("auth/login", new { email, password });

        if (status.IsSuccessStatusCode)
        {
            var token = await status.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthResponse>(token);
            await _accessTokenService.SetToken(result.AccessToken);

            return true;
        }

        return false;
    }

    public async Task Logout()
    {
        
    }
}