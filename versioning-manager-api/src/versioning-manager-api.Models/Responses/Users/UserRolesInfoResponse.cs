namespace versioning_manager_api.Models.Responses.Users;

public class UserRolesInfoResponse
{
    public required IReadOnlyCollection<UserRoleInfo> Roles { get; init; }
}

public class UserRoleInfo
{
    public required string Name { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}