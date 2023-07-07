namespace TempMaiSe.Blazor;

public record Provider(string Name, string Assembly)
{
    public static Provider InMemory = new (nameof(InMemory), typeof(Provider).Assembly.GetName().Name!);
    public static Provider Sqlite = new (nameof(Sqlite), typeof(Sqlite.Marker).Assembly.GetName().Name!);
    public static Provider PostgreSql = new (nameof(PostgreSql), typeof(PostgreSql.Marker).Assembly.GetName().Name!);
    public static Provider SqlServer = new (nameof(SqlServer), typeof(SqlServer.Marker).Assembly.GetName().Name!);
}
