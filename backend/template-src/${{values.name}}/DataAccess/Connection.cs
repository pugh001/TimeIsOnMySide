using Npgsql;

namespace RestService.DataAccess;

/// <summary>
/// Connection class with two connection strings defined for the CQRS pattern.
/// </summary>
public static class Connection
{
    [field: ThreadStatic]
    internal static NpgsqlDataSourceBuilder ReadOnlyConnectionBuilder { get; set; }

    [field: ThreadStatic]
    internal static NpgsqlDataSourceBuilder ReadWriteConnectionBuilder { get; set; }

    internal static NpgsqlConnection ReadOnlyConnection
    {
        get
        {
            var dataSource = ReadOnlyConnectionBuilder.Build();
            return dataSource.CreateConnection();
        }
    }

    internal static NpgsqlConnection ReadWriteConnection
    {
        get
        {
            var dataSource = ReadWriteConnectionBuilder.Build();
            return dataSource.CreateConnection();
        }
    }
}