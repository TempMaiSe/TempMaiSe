namespace TempMaiSe.Razor;

public record DbProvider(string Name, string Assembly)
{
    public static DbProvider InMemory { get; } = new(nameof(InMemory), typeof(DbProvider).Assembly.GetName().Name!);
    public static DbProvider Sqlite { get; } = new(nameof(Sqlite), typeof(Sqlite.Marker).Assembly.GetName().Name!);
    public static DbProvider PostgreSql { get; } = new(nameof(PostgreSql), typeof(PostgreSql.Marker).Assembly.GetName().Name!);
    public static DbProvider SqlServer { get; } = new(nameof(SqlServer), typeof(SqlServer.Marker).Assembly.GetName().Name!);
}
