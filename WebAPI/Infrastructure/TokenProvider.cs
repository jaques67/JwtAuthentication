using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;

namespace WebAPI.Infrastructure;

public class TokenProvider
{
    private readonly IConfiguration _configuration;

    public TokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Token GenerateToken(UserAccount userAccount)
    {
        var accessToken = GenerateAccessToken(userAccount);
        var refreshToken = GenerateRefreshToken();
        return new Token { AccessToken = accessToken,  RefreshToken = refreshToken };
    }

    public RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Expires = DateTime.UtcNow.AddMonths(1),
            CreatedDate = DateTime.UtcNow,
            Enabled = true
        };
        
        return refreshToken;
    }

    private string GenerateAccessToken(UserAccount userAccount)
    {
        var secretKey = _configuration.GetSection("JWT:SecretKey").Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, userAccount.Email),
                new Claim(ClaimTypes.Role, userAccount.Role)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(1),
            SigningCredentials = credentials,
            Issuer = _configuration.GetSection("JWT:Issuer").Value,
            Audience = _configuration.GetSection("JWT:Audience").Value,
        };
        
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}

public class Token
{
    public string AccessToken { get; set; }
    
    public RefreshToken RefreshToken { get; set; }
}

