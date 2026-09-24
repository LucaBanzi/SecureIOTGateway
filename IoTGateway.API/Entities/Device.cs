namespace IoTGateway.API.Entities;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TelemetryData> Telemetries { get; set; } = new List<TelemetryData>();
}