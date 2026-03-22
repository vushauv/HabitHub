using System.ComponentModel.DataAnnotations;

namespace backend.Configuration;

public class AppSettings
{
    [Required]
    public required string PostgresHost { get; set; }

    [Range(1, 65535)]
    public int PostgresPort { get; set; } = 5432;

    [Required]
    public required string PostgresDb { get; set; }

    [Required]
    public required string PostgresUser { get; set; }

    [Required, MinLength(1)]
    public required string PostgresPassword { get; set; }
    
    [Range(1, 65535)]
    public int AppPort { get; set; } = 5000;

    [Required]
    public string CorsOrigins { get; set; } = "http://localhost:3000";

    public string ConnectionString =>
        $"Host={PostgresHost};Port={PostgresPort};Database={PostgresDb};Username={PostgresUser};Password={PostgresPassword}";
}
