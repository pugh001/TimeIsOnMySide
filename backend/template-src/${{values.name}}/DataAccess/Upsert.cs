using System.Net.Sockets;
using Dapper;
using Npgsql;
using RestService.Common.Exceptions;
using RestService.DataAccess.Entity;

namespace RestService.DataAccess;

/// <summary>
/// Static class containing all Inserts
/// Data Access example using Dapper
/// </summary>
public static class Upsert
{
    public static async Task UpsertExample(ExampleEntity exampleEntity)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "@SomeField1", exampleEntity.SomeField1 },
            { "@SomeField2", exampleEntity.SomeField2 },
            { "@SomeField3", exampleEntity.SomeField3 },
            { "@created_on", DateTime.UtcNow }
        };

        // Create a db connection and transaction to ensure data integrity.
        // Use the correct connection type. ReadOnly or ReadWrite
        using (var connection = Connection.ReadWriteConnection)
        {
            await connection.OpenAsync().ConfigureAwait(false);
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var parameters = new DynamicParameters(dictionary);

                bool result =
                    await connection.ExecuteAsync(SQL.Upsert_Something, parameters, transaction) >
                    0;

                if (!result)
                    throw new
                        CustomDatabaseException("DB error - no records updated for an unknown reason");

                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (NpgsqlException npgsqlException)
            {
                if (npgsqlException.InnerException != null &&
                    npgsqlException.InnerException is SocketException)
                    throw new CustomDatabaseException("Error connecting to database",
                                                      innerException: npgsqlException);

                throw new CustomDatabaseException("General DB exception.",
                                                  innerException: npgsqlException);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw new CustomDatabaseException("General DB exception.", innerException: e);
            }
        }
    }
}