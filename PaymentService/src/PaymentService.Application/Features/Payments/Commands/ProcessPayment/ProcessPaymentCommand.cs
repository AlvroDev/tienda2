using MediatR;
using PaymentService.Application.Features.Payments.Dtos;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(Guid OrderId, decimal Amount) : IRequest<PaymentResponse>;