using MediatR;
using PaymentService.Application.Contracts;
using PaymentService.Application.Features.Payments.Dtos;
using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
{
    private readonly IPaymentRepository _repository;

    public ProcessPaymentCommandHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // Si esta orden ya se procesó, devolvemos el mismo resultado (no se cobra dos veces)
        var existing = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
            return PaymentResponse.FromEntity(existing);

        // La regla de aprobación la aplica el dominio
        var payment = Payment.Process(request.OrderId, Money.Create(request.Amount));

        await _repository.AddAsync(payment, cancellationToken);

        return PaymentResponse.FromEntity(payment);
    }
}