namespace cmsUserManagment.Application.DTO;

public class RegisterDeviceTokenRequest
{
    public required string Token { get; set; }

    public string? Platform { get; set; }
}
