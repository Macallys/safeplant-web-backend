namespace DeviceEdgeManagement.Infrastructure;

public sealed class PlantEdgeSettings
{
    public PlantEdgeSettings(string mqttHost, int mqttPort, Uri ingestUrl, string plantKey, string sqlite)
    {
        MqttHost = mqttHost;
        MqttPort = mqttPort;
        IngestUrl = ingestUrl;
        PlantKey = plantKey;
        Sqlite = sqlite;
    }

    public string MqttHost { get; }

    public int MqttPort { get; }

    public Uri IngestUrl { get; }

    public string PlantKey { get; }

    public string Sqlite { get; }
}
