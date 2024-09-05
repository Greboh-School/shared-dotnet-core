namespace School.Shared.Core.Abstractions.Options;

public class ConfigOptions
{
    public static string Section => "Config";
    
    public ServiceOptions Service { get; set; } = new();
    public InclusionOptions Inclusion { get; set; } = new();
    
    public AuthOptions Auth { get; set; } = new();

}