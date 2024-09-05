using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace School.Shared.Core.Authentication.Claims;

public class ClaimAccessHandler : AuthorizationHandler<ClaimRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequirement requirement)
    {
        if (Validate(requirement, context.User.Claims))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    public static bool Validate(ClaimRequirement requirement, IEnumerable<Claim> claims)
    {
        foreach (var type in requirement.Claim)
        {
            var claim = claims.FirstOrDefault(x => x.Type == type);
            
            if (claim is null)
            {
                continue;
            }

            return ValidateClaimLevel(claim, requirement.Level);
        }

        return false;
    }

    public static bool ValidateClaimLevel(Claim claim, int requiredLevel)
    {
        return Enum.TryParse(claim.Value, out ClaimLevel level) && (int)level >= requiredLevel;
    }

    public static bool ValidateClaimLevel(Claim claim, ClaimLevel requiredLevel)
    {
       return ValidateClaimLevel(claim, (int)requiredLevel);
    }
}