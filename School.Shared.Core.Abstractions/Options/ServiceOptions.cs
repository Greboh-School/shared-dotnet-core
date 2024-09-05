namespace School.Shared.Core.Abstractions.Options;

public class ServiceOptions
{
    public static string Section => "Config:Service";
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; } = default;
    public string Route { get; set; } = default!;
    public bool Debug { get; set; } = false;
}