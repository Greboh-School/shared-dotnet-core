using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using School.Shared.Core.Authentication.Claims;

namespace School.Shared.Core.Authentication.Extensions;

public static class AuthorizationOptionsExtensions
{
    public static void AddPolicies(this AuthorizationOptions options)
    {
        var levels = Enum.GetValues<ClaimLevel>();

        foreach (var serviceClaim in ServiceClaim.Claims.Where(serviceClaim => !string.IsNullOrEmpty(serviceClaim.Policy)))
        {
            foreach (var level in levels)
            {
                options.AddPolicy($"{serviceClaim.Policy}:{Enum.GetName(level)}", config =>
                {
                    config.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    config.RequireAuthenticatedUser();
                    config.AddRequirements(new ClaimRequirement(serviceClaim.Claim, level));
                });
            }
        }
        
        options.DefaultPolicy = new AuthorizationPolicyBuilder([JwtBearerDefaults.AuthenticationScheme])
            .RequireAuthenticatedUser()
            .Build();
    }
}