using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DiscountService
{
    private readonly string _connStr;
    private IConfiguration Configuration;

    public DiscountService(IConfiguration configuration)
    {
        Configuration = configuration;
        _connStr = Configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<List<string>> GenerateCodesAsync(ushort count)
    {
        var newCodes = new List<string>();
        var rnd = new Random();

        using var conn = new SqlConnection(_connStr);       
        await conn.OpenAsync();
       

        while (newCodes.Count < count)
        {
            var code = GenerateRandomCode(rnd.Next(7, 9));
            var sql = "IF NOT EXISTS (SELECT 1 FROM DiscountCodes WHERE Code = @Code) " +
                      "INSERT INTO DiscountCodes (Code) VALUES (@Code)";
            var rows = await conn.ExecuteAsync(sql, new { Code = code });

            if (rows > 0)
                newCodes.Add(code); 
        }

        return newCodes;
    }

    public async Task<bool> UseCodeAsync(string code)
    {
        var sql = @"
            UPDATE DiscountCodes
            SET IsUsed = 1
            WHERE Code = @Code AND IsUsed = 0";

        using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var affected = await conn.ExecuteAsync(sql, new { Code = code });

        return affected > 0;
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}
