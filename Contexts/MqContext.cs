namespace mqTests2.Contexts;

public class MqContext
{
    public string? LastSentTextMessage { get; set; }
    public List<string?> AllSentTextMessages { get; set; } = [];
    
    public string? LastReceivedTextMessage { get; set; }
    public List<string?> AllReceivedTextMessages { get; set; } = [];
    
    public byte[]? LastReceivedBytesMessage { get; set; }
    public List<byte[]?> AllReceivedBytesMessages { get; set; } = [];
    
    public string? QueueName { get; set; }
    public string? MsgType { get; set; }
    public string? MsgContentText { get; set; }
    public byte[]? MsgContentBinary { get; set; }
    public int? MsgPriority { get; set; }
    public Guid? MsgCorrelationId { get; set; }
    public string? MsgStringPropertyType { get; set; }
    public string? MsgStringPropertyValue { get; set; }
    
}