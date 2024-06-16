namespace Identity.API.Permissions;

public interface IPermissionService
{
    Task<HashSet<string>> GetPermissionsAsync(Guid clientId);
}
