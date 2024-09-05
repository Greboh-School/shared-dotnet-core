namespace School.Shared.Core.Abstractions.Options;

public class AuthOptions
{
    public static string Section => "Config:Auth";
    
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
}