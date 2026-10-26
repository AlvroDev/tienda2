using MediatR;
using PaymentService.Application.Features.Payments.Dtos;

namespace PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentResponse>;