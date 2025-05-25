using Microsoft.AspNetCore.Authorization;

namespace Orchestration.ServiceDefaults.Authorize;
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return;
        }
        var permissionss = context.User.Claims.Where(x => x.Type == "Permission" &&
                                                          x.Value.Contains(requirement.Permission));
        if (permissionss.Any())
        {
            context.Succeed(requirement);
            return;
        }
        await Task.CompletedTask;
    }
}