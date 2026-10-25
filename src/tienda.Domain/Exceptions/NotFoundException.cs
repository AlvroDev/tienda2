namespace tienda.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} con Id '{id}' no encontrado.") { }
}