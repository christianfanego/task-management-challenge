namespace TaskManagement.Application.Ports;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email);
}
