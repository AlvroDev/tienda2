namespace tienda.Infrastructure.ExternalServices;

// Contrato HTTP con el PaymentService (se serializa con System.Text.Json)
public record ProcessPaymentRequest(Guid OrderId, decimal Amount);

public record ProcessPaymentResponse(Guid OrderId, decimal Amount, string Status, string TransactionId);