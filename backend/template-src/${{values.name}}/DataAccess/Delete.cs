using Dapper;

namespace RestService.DataAccess;

/// <summary>
/// Static class containing all Deletes
/// Data Access example using Dapper
/// </summary>
public static class Delete
{
    public static async Task<bool> DeleteExample(Guid Id)
    {
        var dictionary = new Dictionary<string, object> { { "@Id", Id } };
        var parameters = new DynamicParameters(dictionary);

        // Note the use of the read and write connection
        using var connection = Connection.ReadWriteConnection;
        return await connection.ExecuteAsync(SQL.Delete_something, parameters) > 0;
    }
}