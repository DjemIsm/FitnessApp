using Npgsql;

namespace FitnessApp.Api;

public static class RenderDatabaseUrl
{
    public static string? ToNpgsqlConnectionString(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return null;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : "",
            SslMode = SslMode.Require,
        };

        return builder.ConnectionString;
    }
}