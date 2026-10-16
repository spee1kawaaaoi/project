using Microsoft.AspNetCore.Mvc;
using Npgsql;
using localhost.Models;


namespace localhost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly string _connectionString;

    
    public UsersController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string not found.");
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = new List<User>();

        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        
        var sql = "SELECT id, username, email, created_at FROM users";
        

        
        await using var command = new NpgsqlCommand(sql, connection);

       
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            });
        }

        return Ok(users);
    }
}
