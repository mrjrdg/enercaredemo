using AndrewApi.Handler;
using Microsoft.AspNetCore.Authorization;

namespace AndrewAPI.Extension
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, string scope)
        {
            return builder.AddRequirements(new ScopeRequirement(scope));
        }
    }
}
