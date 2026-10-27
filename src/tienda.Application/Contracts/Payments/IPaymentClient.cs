namespace tienda.Application.Contracts.Payments;

public interface IPaymentClient
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken ct = default);
}

public record PaymentResult(string Status, string TransactionId)
{
    public bool IsApproved => Status == "Approved";
}