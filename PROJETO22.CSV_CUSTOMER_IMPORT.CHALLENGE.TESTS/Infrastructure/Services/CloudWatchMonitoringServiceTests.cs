using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.Services
{
    public class CloudWatchMonitoringServiceTests
    {
        private readonly Mock<IAmazonCloudWatch> _cloudWatchMock;
        private readonly Mock<IAmazonCloudWatchLogs> _cloudWatchLogsMock;
        private readonly CloudWatchMonitoringService _service;

        public CloudWatchMonitoringServiceTests()
        {
            _cloudWatchMock = new Mock<IAmazonCloudWatch>();
            _cloudWatchLogsMock = new Mock<IAmazonCloudWatchLogs>();
            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Aws:LogGroup"] = "TestLogGroup", ["Aws:MetricNamespace"] = "TestNamespace" }).Build();
            _service = new CloudWatchMonitoringService(_cloudWatchMock.Object, _cloudWatchLogsMock.Object, configuration);
        }

        [Fact]
        public async Task LogAsync_ShouldCreateResourcesAndWriteLogs()
        {
            _cloudWatchLogsMock.Setup(x => x.CreateLogGroupAsync(It.IsAny<CreateLogGroupRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogGroupResponse());
            _cloudWatchLogsMock.Setup(x => x.CreateLogStreamAsync(It.IsAny<CreateLogStreamRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogStreamResponse());
            _cloudWatchLogsMock.Setup(x => x.PutLogEventsAsync(It.IsAny<PutLogEventsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutLogEventsResponse());

            await _service.LogAsync("test-stream", "test message", CancellationToken.None);

            _cloudWatchLogsMock.Verify(x => x.CreateLogGroupAsync(It.Is<CreateLogGroupRequest>(r => r.LogGroupName == "TestLogGroup"), It.IsAny<CancellationToken>()), Times.Once);
            _cloudWatchLogsMock.Verify(x => x.CreateLogStreamAsync(It.Is<CreateLogStreamRequest>(r => r.LogGroupName == "TestLogGroup" && r.LogStreamName == "test-stream"), It.IsAny<CancellationToken>()), Times.Once);
            _cloudWatchLogsMock.Verify(x => x.PutLogEventsAsync(It.Is<PutLogEventsRequest>(r => r.LogGroupName == "TestLogGroup" && r.LogStreamName == "test-stream" && r.LogEvents.Count == 1 && r.LogEvents[0].Message == "test message"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublishMetricAsync_ShouldPublishMetricData()
        {
            _cloudWatchMock.Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricDataResponse());

            await _service.PublishMetricAsync("MyMetric", 5);

            _cloudWatchMock.Verify(x => x.PutMetricDataAsync(It.Is<PutMetricDataRequest>(r => r.Namespace == "TestNamespace" && r.MetricData.Count == 1 && r.MetricData[0].MetricName == "MyMetric" && r.MetricData[0].Value == 5 && r.MetricData[0].Unit == Amazon.CloudWatch.StandardUnit.Count), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateFailureAlarmAsync_ShouldCreateAlarm()
        {
            _cloudWatchMock.Setup(x => x.PutMetricAlarmAsync(It.IsAny<PutMetricAlarmRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricAlarmResponse());

            await _service.CreateFailureAlarmAsync("failureAlarm");

            _cloudWatchMock.Verify(x => x.PutMetricAlarmAsync(It.Is<PutMetricAlarmRequest>(r => r.AlarmName == "failureAlarm" && r.MetricName == "Failures" && r.Namespace == "TestNamespace"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateQueueBacklogAlarmAsync_ShouldCreateAlarm()
        {
            _cloudWatchMock.Setup(x => x.PutMetricAlarmAsync(It.IsAny<PutMetricAlarmRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricAlarmResponse());

            await _service.CreateQueueBacklogAlarmAsync("queueAlarm", "myQueue", 10);

            _cloudWatchMock.Verify(x => x.PutMetricAlarmAsync(It.Is<PutMetricAlarmRequest>(r => r.AlarmName == "queueAlarm" && r.Namespace == "AWS/SQS" && r.MetricName == "ApproximateNumberOfMessagesVisible" && r.Dimensions.Exists(d => d.Name == "QueueName" && d.Value == "myQueue") && r.Threshold == 10), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateExceptionAlarmAsync_ShouldCreateAlarm()
        {
            _cloudWatchMock.Setup(x => x.PutMetricAlarmAsync(It.IsAny<PutMetricAlarmRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricAlarmResponse());

            await _service.CreateExceptionAlarmAsync("exceptionAlarm");

            _cloudWatchMock.Verify(x => x.PutMetricAlarmAsync(It.Is<PutMetricAlarmRequest>(r => r.AlarmName == "exceptionAlarm" && r.MetricName == "CriticalExceptions" && r.Namespace == "TestNamespace"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}