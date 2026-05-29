namespace cmsUserManagment.Application.Common;

public enum AppRole
{
    User,
    Admin
}

public static class AppRoles
{
    public const string User = "user";
    public const string Admin = "admin";

    public static string FromEnum(AppRole role) => role.ToString().ToLower();
}
