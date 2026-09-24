namespace IoTGateway.API.Entities;

public class TelemetryData
{
    public long Id { get; set; }
    
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    
    public string MetricName { get; set; } = string.Empty; // es. "temperature", "humidity"
    public double Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}