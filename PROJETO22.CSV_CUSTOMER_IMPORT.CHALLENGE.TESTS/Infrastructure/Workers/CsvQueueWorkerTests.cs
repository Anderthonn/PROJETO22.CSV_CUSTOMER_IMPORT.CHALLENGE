using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Workers;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.Workers
{
    public class CsvQueueWorkerTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Process_Message()
        {
            Dictionary<string, string?> settings = new Dictionary<string, string?>
            {
                {"Aws:QueueUrl", "https://sqs.test/queue"},
                {"CsvQueueWorker:DegreeOfParallelism", "1"}
            };

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            Mock<ICsvProcessorService> processorMock = new Mock<ICsvProcessorService>();
            Mock<IAmazonSQS> amazonSQSMock = new Mock<IAmazonSQS>();
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            CsvMessageDTO csvMessageDTO = new CsvMessageDTO { RequestId = Guid.NewGuid(), S3Key = "file.csv" };
            Message message = new Message
            {
                Body = JsonSerializer.Serialize(csvMessageDTO),
                ReceiptHandle = "handle",
                MessageId = "1"
            };

            amazonSQSMock.SetupSequence(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message> { message } }).ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message>() });

            amazonSQSMock.Setup(s => s.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback(() => cancellationTokenSource.Cancel()).ReturnsAsync(new DeleteMessageResponse());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICsvProcessorService))).Returns(processorMock.Object);

            Mock<IServiceScope> scopeMock = new Mock<IServiceScope>();
            scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            Mock<IServiceScopeFactory> scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            processorMock.Setup(p => p.ProcessAsync(It.IsAny<CsvMessageDTO>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

            Mock<IAmazonCloudWatch> cwMock = new Mock<IAmazonCloudWatch>();
            cwMock.Setup(c => c.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricDataResponse());
            cwMock.Setup(c => c.PutMetricAlarmAsync(It.IsAny<PutMetricAlarmRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricAlarmResponse());

            Mock<IAmazonCloudWatchLogs> logsMock = new Mock<IAmazonCloudWatchLogs>();
            logsMock.Setup(l => l.PutLogEventsAsync(It.IsAny<PutLogEventsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutLogEventsResponse());
            logsMock.Setup(l => l.CreateLogGroupAsync(It.IsAny<CreateLogGroupRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogGroupResponse());
            logsMock.Setup(l => l.CreateLogStreamAsync(It.IsAny<CreateLogStreamRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogStreamResponse());

            CloudWatchMonitoringService monitoring = new CloudWatchMonitoringService(cwMock.Object, logsMock.Object, configuration);

            TestCsvQueueWorker worker = new TestCsvQueueWorker(scopeFactoryMock.Object, amazonSQSMock.Object, configuration, monitoring);

            await worker.PublicExecuteAsync(cancellationTokenSource.Token);

            processorMock.Verify(p => p.ProcessAsync(It.IsAny<CsvMessageDTO>(), It.IsAny<CancellationToken>()), Times.Once);
            amazonSQSMock.Verify(s => s.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            amazonSQSMock.Verify(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        private class TestCsvQueueWorker : CsvQueueWorker
        {
            public TestCsvQueueWorker(IServiceScopeFactory scopeFactory, IAmazonSQS amazonSQS, IConfiguration configuration, CloudWatchMonitoringService monitoring) : base(scopeFactory, amazonSQS, configuration, monitoring) { }
            public Task PublicExecuteAsync(CancellationToken cancellationToken) => base.ExecuteAsync(cancellationToken);
        }
    }
}