using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Contracts.Authentication;
using tienda.Application.Common.Exceptions;
using tienda.Application.Settings;

namespace tienda.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(IUserRepository userRepository, IAuthService authService,
                               IPasswordHasher passwordHasher, JwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _authService = authService;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new InvalidCredentialsException();

        // Verificar contraseña
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        // Verificar que el usuario esté activo
        if (!user.IsActive)
            throw new InvalidCredentialsException("El usuario está desactivado.");

        // Generar JWT
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var token = _authService.GenerateToken(
            user.Id.ToString(),
            user.Email.Value,
            user.Role.ToString(),
            expiresAt
        );

        return new AuthResponse
        {
            Token = token,
            Email = user.Email.Value,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
    }
}
