using Dapper;
using Microsoft.Data.SqlClient;
using WebAPI.Models;

namespace WebAPI.Infrastructure;

public class DataAccess : IDisposable
{
    private SqlConnection? _connection;
    
    public DataAccess(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        if (_connection == null)
        {
            return;
        }

        _connection.Dispose();
        _connection = null;
    }

    public bool RegisterUser(string email, string password, string role)
    {
        var accountCount = _connection.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM [UserAccount] WHERE [Email] = @email", 
            new { email }
        );
        
        if (accountCount > 0) return false;
        
        var sql = "INSERT INTO [UserAccount] (Email, Password, Role) VALUES (@email, @password, @role)";
        var result = _connection.Execute(sql, new {email, password, role});
        
        return result > 0;
    }

    public UserAccount? FindUserByEmail(string email)
    {
        var sql = "SELECT * FROM [UserAccount] WHERE [Email] = @email";
        return _connection.QueryFirstOrDefault<UserAccount>(sql, new {email});
    }
    
    public bool InsertRefreshToken(RefreshToken refreshToken, string email)
    {
        var sql = "INSERT INTO [RefreshToken] (Token, CreatedDate, Expires, Enabled, Email) VALUES (@token, @createddate, @expires, @enabled, @email)";

        var result = _connection.Execute(sql,
            new
            {
                refreshToken.Token,
                refreshToken.CreatedDate,
                refreshToken.Expires,
                refreshToken.Enabled,
                email
            });
        return result > 0;
    }
    
    public bool DisableUserTokenByEmail(string email)
    {
        var sql = "UPDATE [RefreshToken] SET [Enabled] = 0 WHERE [Email] = @email";
        
        var result = _connection.Execute(sql, new {email});
        return result > 0;
    }
    
    public bool DisableUserToken(string token)
    {
        var sql = "UPDATE [RefreshToken] SET [Enabled] = 0 WHERE [Token] = @token";
        
        var result = _connection.Execute(sql, new {token});
        return result > 0;
    }
    
    public bool IsRefreshTokenValid(string token)
    {
        var sql = "SELECT [Enabled] FROM [RefreshToken] WHERE [Token] = @token AND [Enabled] = 1 AND [Expires] >= CAST(GETDATE() AS DATE)";
        
        var result = _connection.ExecuteScalar<int>(sql, new {token});
        return result > 0;
    }
    
    public UserAccount? FindUserByToken(string token)
    {
        var sql = "SELECT [UserAccount].* FROM [RefreshToken] JOIN [UserAccount] ON [RefreshToken].Email = [UserAccount].Email WHERE [Token] = @token";
        
        return _connection.QueryFirstOrDefault<UserAccount>(sql, new {token});
    }
}