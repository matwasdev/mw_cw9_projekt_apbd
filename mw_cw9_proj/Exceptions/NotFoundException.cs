namespace mw_cw9_proj.Exceptions;

public class NotFoundException : BadRequestException
{
    public NotFoundException() {}
    public NotFoundException(string message) : base(message) {}
    
}