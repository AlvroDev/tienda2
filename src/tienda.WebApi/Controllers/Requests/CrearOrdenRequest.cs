namespace tienda.WebApi.Controllers.Requests;

public record CrearOrdenRequest(List<CrearOrdenItemRequest> Items);

public record CrearOrdenItemRequest(Guid ProductoId, int Cantidad);