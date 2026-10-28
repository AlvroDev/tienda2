using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PaymentService.Api.Filters;

// Protege la llamada entre servicios: exige el header X-Api-Key con la clave configurada.
// Es el equivalente a [Authorize], pero con una clave compartida en vez de un JWT.
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    public const string HeaderName = "X-Api-Key";
    private readonly string _apiKey;

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _apiKey = configuration["ServiceAuth:ApiKey"]
            ?? throw new InvalidOperationException("Falta configurar ServiceAuth:ApiKey en appsettings.");
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue(HeaderName, out var providedKey) || providedKey.ToString() != _apiKey)
        {
            // Sin clave o clave incorrecta: 401 y el controller ni se ejecuta
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Falta la API key o es incorrecta.",
                Instance = context.HttpContext.Request.Path
            });
        }
    }
}