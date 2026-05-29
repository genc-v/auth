namespace cmsUserManagment.Application.Common.Settings;

public class RoleSettings
{
    /// <summary>Role name assigned to every new user on registration. If null/empty, no default role is assigned.</summary>
    public string? DefaultRole { get; set; }

    /// <summary>Role name that grants admin access. Used for cache and token checks.</summary>
    public string? AdminRole { get; set; }
}
