using FluentValidation;

namespace tienda.Application.Features.Ordenes.Commands.CrearOrden;

public class CrearOrdenCommandValidator : AbstractValidator<CrearOrdenCommand>
{
    public CrearOrdenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es obligatorio");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe tener al menos un producto");

        // Valida cada elemento de la lista
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("El producto es obligatorio");

            item.RuleFor(i => i.Cantidad)
                .InclusiveBetween(1, 100).WithMessage("La cantidad debe estar entre 1 y 100");
        });
    }
}