namespace cmsUserManagment.Application.DTO;

public class TwoFactorCodeInput
{
    public Guid loginId { get; set; }
    public required string code { get; set; }
}
