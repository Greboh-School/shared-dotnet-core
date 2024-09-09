namespace School.Shared.Core.Abstractions.Options;

public class InclusionOptions
{
    public static string Section => "Config:Inclusion";
    
    public bool MVC { get; set; } = true;
    public bool Auth { get; set; } = true;
    public bool Swagger { get; set; } = true;
    public bool Versioning { get; set; } = true;
    public bool Mapper { get; set; } = true;
    public bool Cors { get; set; } = true;
    public bool Migrator { get; set; } = true;
}