namespace School.Shared.Core.Authentication.Claims;

public class ServiceClaim
{
    private static List<ServiceClaim>? _claims;

    public static List<ServiceClaim> Claims
    {
        get
        {
            return _claims ??= ClaimDefinitions.Build();
        }
    }
    
    public string Claim { get; init; }
    public string Description { get; set; }
    public string Name { get; set; }
    public string Policy { get; set; }
}