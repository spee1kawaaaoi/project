
using localhost.Models;
using Npgsql;
using System.Transactions;

namespace localhost.Services;


public class TransactionService
{
    private readonly string _connectionString;

    public TransactionService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    
    public async Task<List<Transaction>> GetAllAsync()
    {
        var result = new List<Transaction>();

       
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        
        string sql = @"
            SELECT t.id, t.name, t.category_id, c.name AS category_name,
                   t.type, t.amount, t.date, t.comment
            FROM transactions t
            INNER JOIN categories c ON t.category_id = c.id
            ORDER BY t.date DESC";

       
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        // 4. Читаем построчно
        while (await reader.ReadAsync())
            result.Add(MapReader(reader));

        return result;
    }

    // ============ GET BY ID ============
    public async Task<Transaction?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            SELECT t.id, t.name, t.category_id, c.name AS category_name,
                   t.type, t.amount, t.date, t.comment
            FROM transactions t
            INNER JOIN categories c ON t.category_id = c.id
            WHERE t.id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);   

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapReader(reader);

        return null;
    }

  
    public async Task<int> AddAsync(Transaction transaction)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO transactions (name, category_id, type, amount, date, comment)
            VALUES (@name, @category_id, @type, @amount, @date, @comment)
            RETURNING id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", transaction.Name);
        command.Parameters.AddWithValue("@category_id", transaction.CategoryId);
        command.Parameters.AddWithValue("@type", transaction.Type);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@date", transaction.Date);
        command.Parameters.AddWithValue("@comment", (object?)transaction.Comment ?? DBNull.Value);

        
        return (int)(await command.ExecuteScalarAsync())!;
    }

    
    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            UPDATE transactions 
            SET name = @name, category_id = @category_id, type = @type,
                amount = @amount, date = @date, comment = @comment
            WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", transaction.Id);
        command.Parameters.AddWithValue("@name", transaction.Name);
        command.Parameters.AddWithValue("@category_id", transaction.CategoryId);
        command.Parameters.AddWithValue("@type", transaction.Type);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@date", transaction.Date);
        command.Parameters.AddWithValue("@comment", (object?)transaction.Comment ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync() > 0;
    }

   
    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "DELETE FROM transactions WHERE id = @id";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    
    public async Task<object> GetStatisticsAsync(DateTime? start, DateTime? end)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN type = 'Доход'  THEN amount ELSE 0 END), 0) AS total_income,
                COALESCE(SUM(CASE WHEN type = 'Расход' THEN amount ELSE 0 END), 0) AS total_expense
            FROM transactions
            WHERE (@start IS NULL OR date >= @start)
              AND (@end   IS NULL OR date <= @end)";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@start", (object?)start ?? DBNull.Value);
        command.Parameters.AddWithValue("@end", (object?)end ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        decimal income = reader.GetDecimal(0);
        decimal expense = reader.GetDecimal(1);

        return new
        {
            TotalIncome = income,
            TotalExpense = expense,
            Balance = income - expense
        };
    }

  
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var result = new List<Category>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT id, name, created_at FROM categories ORDER BY name";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2)
            });
        }

        return result;
    }

    
    private static Transaction MapReader(NpgsqlDataReader reader)
    {
        return new Transaction
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            CategoryId = reader.GetInt32(2),
            CategoryName = reader.GetString(3),
            Type = reader.GetString(4),
            Amount = reader.GetDecimal(5),
            Date = reader.GetDateTime(6),
            Comment = reader.IsDBNull(7) ? null : reader.GetString(7)
        };
    }
}
