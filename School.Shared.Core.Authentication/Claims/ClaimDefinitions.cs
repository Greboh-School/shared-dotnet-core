namespace School.Shared.Core.Authentication.Claims;

public static class ClaimDefinitions
{
    public static List<ServiceClaim> Build()
    {
        List<ServiceClaim> claims = [];

        claims.Add(new()
        {
            Name = "Website Role",
            Description = "Role for accessing the website",
            Claim = "systems:website:role",
            Policy = "website"
        });
        
        claims.Add(new()
        {
            Name = "Game Role",
            Description = "Role within the game",
            Claim = "systems:game:role",
            Policy = "game"
        });


        return claims;
    }
}