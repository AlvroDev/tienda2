using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.ValueObjects;

// record: dos Money con el mismo monto y moneda son iguales (igualdad por valor)
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "ARS")
    {
        if (amount <= 0)
            throw new DomainException("El monto del pago debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La moneda es obligatoria.");

        return new Money(amount, currency.ToUpper());
    }
}