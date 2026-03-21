namespace BlazorServer.Services;

public class AccessTokenService(CookieService cookieService)
{
    private readonly CookieService _cookieService = cookieService;
    private readonly string _tokenKey = "access_token";

    public async Task SetToken(string accessToken)
    {
        await _cookieService.Set(_tokenKey, accessToken, 1);
    }

    public async Task<string> GetToken()
    {
        return await _cookieService.Get(_tokenKey);
    }

    public async Task RemoveToken()
    {
        await _cookieService.Remove(_tokenKey);
    }
}