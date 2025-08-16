namespace versioning_manager_api.Routes.StaticStorages;

public static class RolesStorage
{
    public const string CreateUserRole = "User.Create";
    public const string UpdateUserRoleRole = "User.Role.Update";
    public const string DeleteUserRole = "User.Delete";
    public const string ChangePasswordRole = "User.Password.Change";
    public const string GetUsersRole = "User.Info.Get";

    public const string CreateRoleRole = "Role.Create";
    public const string GetSystemRolesRole = "Role.System";
    public const string GetUserRolesRole = "Role.Users";
    public const string UpdateRoleRole = "Role.Update";
    public const string DeleteRoleRole = "Role.Delete";

    public const string CreateDeviceRole = "Device.Create";
    public const string UpdateDeviceRole = "Device.Update";
    public const string DeleteDeviceRole = "Device.Delete";
    public const string ListDeviceRole = "Device.List";

    public const string ProjectCreateRole = "Project.Create";
    public const string GetProjectsRole = "Project.Get";
    public const string ProjectUpdateRole = "Project.Update";

    public const int Count = 17;

    //DO NOT REMEMBER TO UPDATE COUNT CONST FIELD!
    public static IEnumerable<string> Roles { get; } =
    [
        CreateUserRole,
        UpdateUserRoleRole,
        DeleteUserRole,
        ChangePasswordRole,
        GetUsersRole,

        CreateRoleRole,
        GetSystemRolesRole,
        GetUserRolesRole,
        UpdateRoleRole,
        DeleteRoleRole,

        CreateDeviceRole,
        UpdateDeviceRole,
        DeleteDeviceRole,
        ListDeviceRole,

        ProjectCreateRole,
        GetProjectsRole,
        ProjectUpdateRole
    ];
}