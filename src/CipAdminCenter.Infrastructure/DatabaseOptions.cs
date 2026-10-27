using System.ComponentModel.DataAnnotations;

namespace CipAdminCenter.Infrastructure;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Npgsql connection string. Supplied through configuration (e.g. Database__ConnectionString); never committed.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Applies pending EF Core migrations on startup. Intended for local development.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
