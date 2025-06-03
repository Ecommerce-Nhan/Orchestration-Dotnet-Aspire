using Microsoft.AspNetCore.Authorization;

namespace Orchestration.ServiceDefaults.Authorize;
public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permission)
    {
        Policy = permission;
    }
}