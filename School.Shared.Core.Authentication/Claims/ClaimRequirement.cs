using Microsoft.AspNetCore.Authorization;

namespace School.Shared.Core.Authentication.Claims;

public class ClaimRequirement : IAuthorizationRequirement
{
    public string[] Claim { get; }
    public int Level { get; }

    public ClaimRequirement(string claim, ClaimLevel level)
    {
        Claim = GetHierarchy(claim);
        Level = (int)level;
    }
    
    private string[] GetHierarchy(string claim)
    {
        // Initialize the list with the original claim
        List<string> claims = new() { claim };

        // Check if the claim contains a colon, indicating it's a hierarchical claim
        if (claim.Contains(':'))
        {
            // Split the claim into parts based on the colon
            string[] split = claim.Split(':');

            // Iterate from the full length of the split array down to 0
            for (int i = split.Length; i >= 0; i--)
            {
                // Create a new claim by joining the first 'i' elements of the split array
                // This generates all possible parent claims in the hierarchy
                claims.Add(string.Join(':', split[..i]));
            }
        }

        // Return the list of claims, ensuring that:
        // 1. No empty strings are included
        // 2. Duplicate claims are removed
        return claims
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .ToArray();
    }
}