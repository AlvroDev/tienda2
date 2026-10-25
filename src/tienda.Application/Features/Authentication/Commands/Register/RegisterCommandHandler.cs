using MediatR;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Contracts.Authentication;
using tienda.Domain.Entities;
using tienda.Domain.ValueObjects;
using tienda.Domain.Enums;
using tienda.Application.Common.Exceptions;

namespace tienda.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IAuthService authService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el email no exista
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException($"El email {request.Email} ya está registrado.");

        // Hashear contraseña
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Crear usuario (siempre como Customer en registro)
        var email = Email.Create(request.Email);
        var user = User.Create(request.FirstName, request.LastName, email, passwordHash, UserRole.Customer);

        // Guardar en BD
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Generar JWT
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
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
