using Amazon.CloudWatch;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces
{
    public interface ICloudWatchMonitoringService
    {
        Task LogAsync(string stream, string message, CancellationToken cancellationToken);
        Task PublishMetricAsync(string metricName, double value, StandardUnit unit);
        Task CreateFailureAlarmAsync(string alarmName);
        Task CreateQueueBacklogAlarmAsync(string alarmName, string queueName, double threshold);
        Task CreateExceptionAlarmAsync(string alarmName);
    }
}