using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Data;

public class DatabaseHelper
{
    private readonly ApplicationDbContext _context;

    public DatabaseHelper(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(string procedureName, params SqlParameter[] parameters)
    {
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        await using var cmd = new SqlCommand(procedureName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();
        var list = new List<T>();
        while (await reader.ReadAsync())
        {
            list.Add(map(reader));
        }
        return list;
    }
}
