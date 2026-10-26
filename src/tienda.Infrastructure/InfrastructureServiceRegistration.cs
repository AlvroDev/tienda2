using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tienda.Application.Contracts.Persistence;
using tienda.Application.Contracts.Authentication;
using tienda.Application.Settings;
using tienda.Infrastructure.Repositories;
using tienda.Infrastructure.Authentication;

namespace tienda.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Dejamos solo el repositorio, ya que el DbContext lo maneja el Program.cs directo de la API
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Configurar JWT
        var jwtSettings = new JwtSettings
        {
            SecretKey = configuration["JwtSettings:SecretKey"] ?? "",
            Issuer = configuration["JwtSettings:Issuer"] ?? "",
            Audience = configuration["JwtSettings:Audience"] ?? "",
            ExpirationMinutes = int.Parse(configuration["JwtSettings:ExpirationMinutes"] ?? "60")
        };
        services.AddSingleton(jwtSettings);
        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}