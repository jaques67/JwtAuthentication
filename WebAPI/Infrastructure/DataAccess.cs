using Dapper;
using Microsoft.Data.SqlClient;

namespace WebAPI.Infrastructure;

public class DataAccess : IDisposable
{
    private SqlConnection _connection;
    
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
}