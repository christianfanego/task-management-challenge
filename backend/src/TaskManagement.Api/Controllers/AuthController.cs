using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Services;
using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserService _registerService;
    private readonly LoginUserService _loginService;

    public AuthController(RegisterUserService registerService, LoginUserService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _registerService.RegisterAsync(request.Email, request.Password);
            return StatusCode(201, new UserDto(user.Id, user.Email));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { title = "Conflict", status = 409, detail = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _loginService.LoginAsync(request.Email, request.Password);
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userId = Guid.Parse(jwt.Claims.First(c => c.Type == "sub").Value);
            var email = jwt.Claims.First(c => c.Type == "email").Value;

            return Ok(new AuthResponse(
                token,
                "Bearer",
                jwt.ValidTo,
                new UserDto(userId, email)));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { title = "Unauthorized", status = 401, detail = "Authentication required or credentials are invalid." });
        }
    }
}
