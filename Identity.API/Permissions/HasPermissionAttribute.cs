using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Permissions;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission)
        : base(policy: permission.ToString())
    {
        
    }
}
