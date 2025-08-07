using Amazon.CloudWatch.Model;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces
{
    public interface ICloudWatchMonitoringService
    {
        Task LogAsync(string stream, string message, CancellationToken cancellationToken);
        Task PublishMetricAsync(string metricName, double value);
        Task<PutMetricDataResponse> PublishMetricAsync(string metricName, double value, Amazon.CloudWatch.StandardUnit unit);
        Task PublishReprocessedRecordsMetricAsync(int count);
        Task PublishInvalidRecordsAsync(int count);
        Task<PutMetricAlarmResponse> CreateFailureAlarmAsync(string alarmName);
        Task<PutMetricAlarmResponse> CreateQueueBacklogAlarmAsync(string alarmName, string queueName, double threshold);
        Task<PutMetricAlarmResponse> CreateExceptionAlarmAsync(string alarmName);
    }
}