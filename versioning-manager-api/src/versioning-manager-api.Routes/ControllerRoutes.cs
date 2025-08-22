namespace versioning_manager_api.Routes;

public static class ControllerRoutes
{
    public static class DeviceAdministrationV1Routes
    {
        public const string ApiVersion = "1.0";

        /// <summary>
        ///     Replace "{version:apiVersion}" to <see cref="ApiVersion" />.
        /// </summary>
        public const string ControllerRoute = "api/v{version:apiVersion}/device/administration";

        public const string PostDeviceRoute = "device";
        public const string RefreshDeviceRoute = "device/refresh";
        public const string GetDevicesRoute = "devices";
        public const string DeleteDeviceRoute = "device";

        public static string GetBaseRoute()
        {
            return ControllerRoute.Replace("{version:apiVersion}", ApiVersion);
        }
    }

    public static class ProjectAdministrationV1Routes
    {
        public const string ApiVersion = "1.0";

        /// <summary>
        ///     Replace "{version:apiVersion}" to <see cref="ApiVersion" />.
        /// </summary>
        public const string ControllerRoute = "api/v{version:apiVersion}/project/administration";

        public const string PostProjectRoute = "project";
        public const string PostProjectEntryRoute = "project/entry";
        public const string GetProjectsRoute = "projects";

        /// <summary>
        ///     Replace "{name}" to project name.
        /// </summary>
        public const string GetProjectEntriesRoute = "project/{name}/entries";

        /// <summary>
        ///     Replace "{id}" to project entry id.
        /// </summary>
        public const string ChangeProjectEntryActualityRoute = "project/entry/{id}/change/actuality";

        /// <summary>
        ///     Replace "{id}" to project entry id.
        /// </summary>
        public const string GetProjectEntryImagesRoute = "project/entry/{id}/images";

        /// <summary>
        ///     Replace "{id}" to project entry id.
        /// </summary>
        public const string MigrateImagesToAnotherProjectRoute = "project/images/migrate/to/{id}";

        /// <summary>
        ///     Replace "{id}" to image id.
        /// </summary>
        public const string ChangeImageActualityRoute = "project/image/{id}/change/active";

        public static string GetBaseRoute()
        {
            return ControllerRoute.Replace("{version:apiVersion}", ApiVersion);
        }

        public static string GetProjectEntriesRouteWith(string name)
        {
            return GetProjectEntriesRoute.Replace("{name}", name);
        }

        public static string ChangeProjectEntryActualityRouteWith(int id)
        {
            return ChangeProjectEntryActualityRoute.Replace("{id}", id.ToString());
        }

        public static string GetProjectEntryImagesRouteWith(int id)
        {
            return GetProjectEntryImagesRoute.Replace("{id}", id.ToString());
        }

        public static string MigrateImagesToAnotherProjectRouteWith(int id)
        {
            return MigrateImagesToAnotherProjectRoute.Replace("{id}", id.ToString());
        }

        public static string ChangeImageActualityRouteWith(int id)
        {
            return ChangeImageActualityRoute.Replace("{id}", id.ToString());
        }
    }

    public static class ProjectV1Routes
    {
        public const string ApiVersion = "1.0";

        /// <summary>
        ///     Replace "{version:apiVersion}" to <see cref="ApiVersion" />.
        /// </summary>
        public const string ControllerRoute = "api/v{version:apiVersion}/project";

        public const string DownloadImageRoute = "image/file";
        public const string UploadImageRoute = "image/file";
        public const string PostImageInfoRoute = "image";

        /// <summary>
        ///     Replace "{name}" to project name.
        /// </summary>
        public const string GetProjectInfoRoute = "project/{name}/info";

        /// <summary>
        ///     Replace "{id}" to project entry id.
        /// </summary>
        public const string GetProjectComposeFileRoute = "project/{id}/compose";

        public static string GetBaseRoute()
        {
            return ControllerRoute.Replace("{version:apiVersion}", ApiVersion);
        }

        public static string GetProjectInfoRouteWith(string name)
        {
            return GetProjectInfoRoute.Replace("{name}", name);
        }

        public static string GetProjectComposeFileRouteWith(int id)
        {
            return GetProjectComposeFileRoute.Replace("{id}", id.ToString());
        }
    }

    public static class UsersV1Routes
    {
        public const string ApiVersion = "1.0";

        /// <summary>
        ///     Replace "{version:apiVersion}" to <see cref="ApiVersion" />.
        /// </summary>
        public const string ControllerRoute = "api/v{version:apiVersion}/users";

        public const string CreateNewUserRoute = "create/user";
        public const string LoginRoute = "login";
        public const string CreateRoleRoute = "create/role";
        public const string ChangePasswordRoute = "change/password";
        public const string ChangeSelfPasswordRoute = "change/self/password";
        public const string GetSystemRolesRoute = "system/roles";
        public const string GetUserRolesRoute = "user/roles";
        public const string ChangeUsersRoleRoute = "user/role";
        public const string DeleteUserRoleRoute = "user/role";
        public const string UpdateRoleRoute = "role";
        public const string DeleteUserRoute = "user";
        public const string ActivateDeletedUserRoute = "change/user/active";
        public const string GetUserInfoRoute = "user";

        public static string GetBaseRoute()
        {
            return ControllerRoute.Replace("{version:apiVersion}", ApiVersion);
        }
    }
}