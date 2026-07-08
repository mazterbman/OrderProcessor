namespace OrderProcessor.Domain;

public class OrderValidationException : Exception
{
    public OrderValidationException(string message) 
        : base(message)
    {
        
    }
}