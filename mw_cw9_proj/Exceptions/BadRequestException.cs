namespace mw_cw9_proj.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() {}
    public BadRequestException(string message) : base(message) {}
    
}