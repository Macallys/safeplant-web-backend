namespace DeviceEdgeManagement.Infrastructure.Mqtt;

internal static class PlantEdgeTiming
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public static async Task DelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Interval, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
