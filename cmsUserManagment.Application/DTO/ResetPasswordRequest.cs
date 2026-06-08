namespace cmsUserManagment.Application.DTO;

public class ResetPasswordRequest
{
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}
