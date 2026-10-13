using System.Text;
using System.Text.Json;
using IoTGateway.API.Data;
using IoTGateway.API.Entities;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;

namespace IoTGateway.API.Services;

public class MqttBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MqttBackgroundService> _logger;
    private IMqttClient? _mqttClient;

    public MqttBackgroundService(IServiceProvider serviceProvider, ILogger<MqttBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();

        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // Connessione interna broker-backend su 1883
            .WithClientId("IoTGateway_Backend_Worker")
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            _logger.LogInformation("Ricevuto messaggio MQTT su topic '{Topic}': {Payload}", topic, payload);

            await ProcessTelemetryMessageAsync(topic, payload);
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    _logger.LogInformation("Connessione al Broker MQTT in corso...");
                    await _mqttClient.ConnectAsync(mqttOptions, stoppingToken);
                    
                    // Sintassi corretta per MQTTnet 4.x per costruire le opzioni di iscrizione
                    var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter(f => f.WithTopic("devices/+/telemetry"))
                        .Build();

                    await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
                    _logger.LogInformation("Iscritto con successo al topic 'devices/+/telemetry'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore di connessione al broker MQTT. Riprovo tra 5 secondi...");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessTelemetryMessageAsync(string topic, string payload)
    {
        var parts = topic.Split('/');
        if (parts.Length < 3) return;
        var serialNumber = parts[1];

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var device = await dbContext.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serialNumber);
        if (device == null || !device.IsActive)
        {
            _logger.LogWarning("Messaggio scartato: dispositivo '{Serial}' non trovato o disattivato.", serialNumber);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (root.TryGetProperty("metric", out var metricProp) &&
                root.TryGetProperty("value", out var valueProp))
            {
                var telemetry = new TelemetryData
                {
                    DeviceId = device.Id,
                    MetricName = metricProp.GetString() ?? "Unknown",
                    Value = valueProp.GetDouble(),
                    Timestamp = DateTime.UtcNow
                };

                dbContext.Telemetries.Add(telemetry);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Telemetria salvata su DB per dispositivo '{Serial}': {Metric} = {Value}",
                    serialNumber, telemetry.MetricName, telemetry.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il parsing del payload JSON di telemetria.");
        }
    }
}