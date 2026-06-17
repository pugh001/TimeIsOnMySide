using Dapper;
using RestService.DataAccess.Entity;

namespace RestService.DataAccess;

/// <summary>
/// Static class containing all Queries
/// Data Access example using Dapper
/// </summary>
public static class Query
{
    public static ExampleEntity QueryExample()
    {
        // Note the use of the read only connection.
        using var connection = Connection.ReadOnlyConnection;
        return connection.Query<ExampleEntity>(SQL.Query_something).FirstOrDefault();
    }
}