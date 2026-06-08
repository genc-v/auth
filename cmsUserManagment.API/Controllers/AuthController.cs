using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Security;

using Google.Authenticator;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.API.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly HeadersManager _headersManager;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public AuthController(IAuthenticationService authenticationService, IJwtTokenProvider jwtTokenProvider,
        HeadersManager headersManager)
    {
        _authenticationService = authenticationService;
        _jwtTokenProvider = jwtTokenProvider;
        _headersManager = headersManager;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<object> Register([FromBody] RegisterUser newUser)
    {
        var success = await _authenticationService.Register(newUser);
        return new { success, data = (object)null };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> Login([FromBody] LoginUser loginRequest)
    {
        var result = await _authenticationService.Login(loginRequest.Email, loginRequest.Password);
        return new { success = true, data = result };
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> Logout([FromBody] LogoutRequest request)
    {
        string jwt = _headersManager.GetJwtFromHeader(Request.Headers);
        await _authenticationService.Logout(jwt, request.RefreshToken);
        return new { success = true, data = (object)null };
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var jwtToken = await _authenticationService.RefreshToken(request.RefreshToken,
            _headersManager.GetJwtFromHeader(Request.Headers));
        return new { success = true, data = jwtToken };
    }

    [HttpPost("2fa/setup")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> GenerateTwoFactorAuthSetupCode()
    {
        var setupCode = await _authenticationService.GenerateAuthToken(_headersManager.GetJwtFromHeader(Request.Headers));
        return new { success = true, data = setupCode };
    }

    [HttpPost("2fa/confirm")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> TwoFactorAuthenticationConfirm([FromBody] TwoFactorConfirmRequest input)
    {
        var success = await _authenticationService.TwoFactorAuthenticationConfirm(
            _headersManager.GetJwtFromHeader(Request.Headers), input.Code);
        return new { success, data = (object)null };
    }

    [HttpPost("2fa/disable")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> DisableTwoFactorAuth([FromBody] TwoFactorConfirmRequest input)
    {
        var success = await _authenticationService.DisableTwoFactorAuth(_headersManager.GetJwtFromHeader(Request.Headers), input.Code);
        return new { success, data = (object)null };
    }

    [HttpPost("2fa/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> TwoFactorAuthenticationLogin([FromBody] TwoFactorLoginRequest request)
    {
        var credentials = await _authenticationService.TwoFactorAuthenticationLogin(request.LoginId, request.Code);
        return new { success = true, data = new { jwtToken = credentials.jwtToken, refreshToken = credentials.refreshToken } };
    }

    [HttpPut("account")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var success = await _authenticationService.UpdateAccount(_headersManager.GetJwtFromHeader(Request.Headers), request);
        return new { success, data = (object)null };
    }

    [HttpGet("account")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<object> GetAccountInfo()
    {
        var info = await _authenticationService.GetUserInfo(_headersManager.GetJwtFromHeader(Request.Headers));
        return new { success = true, data = info };
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<object> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authenticationService.ForgotPassword(request.Email);
        return new { success = true, message = "If the email exists, a reset code has been sent." };
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<object> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authenticationService.ResetPassword(request.Code, request.NewPassword);
        return new { success = true };
    }
}
