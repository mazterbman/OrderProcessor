namespace OrderProcessor.Domain;

public enum OrderStatus
{
    Pending = 0,
    Processing,
    Completed,
    Failed
}