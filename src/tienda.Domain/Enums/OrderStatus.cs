namespace tienda.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,          // Creada, esperando el pago
    Paid = 2,             // Pago aprobado
    PaymentRejected = 3   // Pago rechazado (se devuelve el stock)
}