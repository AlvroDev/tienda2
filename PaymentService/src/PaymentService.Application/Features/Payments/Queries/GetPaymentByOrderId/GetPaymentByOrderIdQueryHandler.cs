using MediatR;
using PaymentService.Application.Contracts;
using PaymentService.Application.Features.Payments.Dtos;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentResponse>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentResponse> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Pago de la orden", request.OrderId);

        return PaymentResponse.FromEntity(payment);
    }
}