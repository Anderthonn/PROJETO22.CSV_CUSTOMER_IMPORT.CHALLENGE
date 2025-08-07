using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Microsoft.Extensions.Configuration;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services
{
    public class CloudWatchMonitoringService : ICloudWatchMonitoringService
    {
        private readonly IAmazonCloudWatch _cloudWatch;
        private readonly IAmazonCloudWatchLogs _cloudWatchLogs;
        private readonly string _logGroup;
        private readonly string _metricNamespace;

        public CloudWatchMonitoringService(IAmazonCloudWatch cloudWatch, IAmazonCloudWatchLogs cloudWatchLogs, IConfiguration configuration)
        {
            _cloudWatch = cloudWatch;
            _cloudWatchLogs = cloudWatchLogs;
            _logGroup = configuration["Aws:LogGroup"] ?? "CsvImportLogs";
            _metricNamespace = configuration["Aws:MetricNamespace"] ?? "CsvImport";
        }

        public async Task LogAsync(string stream, string message, CancellationToken cancellationToken)
        {
            await EnsureLogResourcesAsync(stream);

            PutLogEventsRequest putLogsRequest = new PutLogEventsRequest
            {
                LogGroupName = _logGroup,
                LogStreamName = stream,
                LogEvents = new List<InputLogEvent>
                {
                    new InputLogEvent { Message = message, Timestamp = DateTime.UtcNow }
                }
            };

            try
            {
                await _cloudWatchLogs.PutLogEventsAsync(putLogsRequest, cancellationToken);
            }
            catch (InvalidSequenceTokenException ex)
            {
                putLogsRequest.SequenceToken = ex.ExpectedSequenceToken;
                await _cloudWatchLogs.PutLogEventsAsync(putLogsRequest, cancellationToken);
            }
        }

        public async Task PublishMetricAsync(string metricName, double value) => await Task.Run(() => (PublishMetricAsync(metricName, value, Amazon.CloudWatch.StandardUnit.Count)));

        public async Task<PutMetricDataResponse> PublishMetricAsync(string metricName, double value, Amazon.CloudWatch.StandardUnit unit)
        {
            MetricDatum metricDatum = new MetricDatum
            {
                MetricName = metricName,
                Value = value,
                Unit = unit
            };

            PutMetricDataRequest putMetricDataRequest = new PutMetricDataRequest
            {
                Namespace = _metricNamespace,
                MetricData = new List<MetricDatum> { metricDatum }
            };

            return await Task.Run(() => (_cloudWatch.PutMetricDataAsync(putMetricDataRequest)));
        }

        public Task PublishReprocessedRecordsMetricAsync(int count)
        {
            return PublishMetricAsync("RecordsReprocessed", count);
        }

        public Task PublishInvalidRecordsAsync(int count)
        {
            return PublishMetricAsync("InvalidRecords", count);
        }

        public async Task<PutMetricAlarmResponse> CreateFailureAlarmAsync(string alarmName)
        {
            PutMetricAlarmRequest putMetricAlarmRequest = new PutMetricAlarmRequest
            {
                AlarmName = alarmName,
                MetricName = "Failures",
                Namespace = _metricNamespace,
                Statistic = Statistic.Sum,
                ComparisonOperator = ComparisonOperator.GreaterThanThreshold,
                Threshold = 1,
                EvaluationPeriods = 3,
                ActionsEnabled = true
            };

            return await Task.Run(() => (_cloudWatch.PutMetricAlarmAsync(putMetricAlarmRequest)));
        }

        public async Task<PutMetricAlarmResponse> CreateQueueBacklogAlarmAsync(string alarmName, string queueName, double threshold)
        {
            PutMetricAlarmRequest putMetricAlarmRequest = new PutMetricAlarmRequest
            {
                AlarmName = alarmName,
                Namespace = "AWS/SQS",
                MetricName = "ApproximateNumberOfMessagesVisible",
                Statistic = Statistic.Average,
                ComparisonOperator = ComparisonOperator.GreaterThanThreshold,
                Threshold = threshold,
                EvaluationPeriods = 1,
                Dimensions = new List<Dimension>
                {
                    new Dimension { Name = "QueueName", Value = queueName }
                },
                ActionsEnabled = true
            };

            return await Task.Run(() => (_cloudWatch.PutMetricAlarmAsync(putMetricAlarmRequest)));
        }

        public async Task<PutMetricAlarmResponse> CreateExceptionAlarmAsync(string alarmName)
        {
            PutMetricAlarmRequest putMetricAlarmRequest = new PutMetricAlarmRequest
            {
                AlarmName = alarmName,
                MetricName = "CriticalExceptions",
                Namespace = _metricNamespace,
                Statistic = Statistic.Sum,
                ComparisonOperator = ComparisonOperator.GreaterThanThreshold,
                Threshold = 1,
                EvaluationPeriods = 1,
                ActionsEnabled = true
            };

            return await Task.Run(() => (_cloudWatch.PutMetricAlarmAsync(putMetricAlarmRequest)));
        }

        private async Task EnsureLogResourcesAsync(string stream)
        {
            try
            {
                CreateLogGroupRequest createLogGroupRequest = new CreateLogGroupRequest
                {
                    LogGroupName = _logGroup
                };
                await _cloudWatchLogs.CreateLogGroupAsync(createLogGroupRequest);
            }
            catch (ResourceAlreadyExistsException) { }

            try
            {
                CreateLogStreamRequest createLogStreamRequest = new CreateLogStreamRequest
                {
                    LogGroupName = _logGroup,
                    LogStreamName = stream
                };
                await _cloudWatchLogs.CreateLogStreamAsync(createLogStreamRequest);
            }
            catch (ResourceAlreadyExistsException) { }
        }
    }
}