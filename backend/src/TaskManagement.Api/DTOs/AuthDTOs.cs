namespace TaskManagement.Api.DTOs;

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAt, UserDto User);
public record UserDto(Guid Id, string Email);
