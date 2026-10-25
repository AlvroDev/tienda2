namespace tienda.Application.Common.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message = "Email o contraseña incorrectos.")
        : base(message) { }
}