using TaskManagement.Application.Ports;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services;

public class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(email, ct);
        if (existing != null)
            throw new InvalidOperationException("Email is already registered.");

        var passwordHash = _passwordHasher.HashPassword(password);
        var user = User.Create(email, passwordHash);
        return await _userRepository.CreateAsync(user, ct);
    }
}
