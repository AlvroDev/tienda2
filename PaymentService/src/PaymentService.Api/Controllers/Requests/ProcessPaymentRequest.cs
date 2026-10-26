namespace PaymentService.Api.Controllers.Requests;

public record ProcessPaymentRequest(Guid OrderId, decimal Amount);