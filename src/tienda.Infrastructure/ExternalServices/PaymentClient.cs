using System.Net.Http.Json;
using tienda.Application.Common.Exceptions;
using tienda.Application.Contracts.Payments;

namespace tienda.Infrastructure.ExternalServices;

public class PaymentClient : IPaymentClient
{
    private const string MensajeNoDisponible =
        "El servicio de pagos no está disponible. La orden sigue pendiente; intentá confirmarla más tarde.";

    private readonly HttpClient _httpClient;

    // El HttpClient lo crea y configura IHttpClientFactory (ver AddHttpClient)
    public PaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/payments/process",
                new ProcessPaymentRequest(orderId, amount),
                ct);

            response.EnsureSuccessStatusCode(); // si no es 2xx, lanza HttpRequestException

            var body = await response.Content.ReadFromJsonAsync<ProcessPaymentResponse>(cancellationToken: ct)
                ?? throw new ServiceUnavailableException(MensajeNoDisponible);

            return new PaymentResult(body.Status, body.TransactionId);
        }
        catch (HttpRequestException ex)
        {
            // Servicio apagado, puerto equivocado o respuesta de error
            throw new ServiceUnavailableException(MensajeNoDisponible, ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // Timeout: el servicio tardó más que lo configurado
            throw new ServiceUnavailableException(MensajeNoDisponible, ex);
        }
    }
}