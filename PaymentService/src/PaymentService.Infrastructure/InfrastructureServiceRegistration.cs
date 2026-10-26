using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Contracts;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Singleton: una sola instancia para toda la vida de la app,
        // así los pagos guardados sobreviven entre requests
        services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
        return services;
    }
}