using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;
using System.Text;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.Services
{
    public class CsvProcessorServiceTests
    {
        [Fact]
        public async Task ProcessAsync_WithValidRecord_ImportsClient()
        {
            string csv = "Name,Cpf,Email\r\n" +
                         "John Doe,52998224725,john@example.com\r\n" +
                         "\"Maria, Clara\",16899535009,maria.clara@example.com\r\n" +
                         "\"Ana \"\"Nina\"\" Souza\",11144477735,nina.souza@example.com\r\n" +
                         "José Ávila,15350946056,jose.avila@example.com";

            Mock<IAmazonS3> amazonS3Mock = new Mock<IAmazonS3>();
            amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() =>
                        {
                            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
                            return new GetObjectResponse { ResponseStream = memoryStream };
                        });

            Mock<IAmazonSQS> amazonSQSMock = new Mock<IAmazonSQS>();
            amazonSQSMock.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SendMessageResponse());

            List<Client>? saved = null;

            Mock<IClientRepository> clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(x => x.ExistsByCpfAsync(It.Is<string>(cpf => cpf != "52998224725"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            clientRepository.Setup(x => x.ExistsByCpfAsync("52998224725", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            clientRepository.Setup(x => x.AddRangeAsync(It.IsAny<List<Client>>(), It.IsAny<CancellationToken>())).Callback<List<Client>, CancellationToken>((list, token) => saved = list).Returns(Task.CompletedTask);

            Mock<IAmazonCloudWatch> amazonCloudWatchMock = new Mock<IAmazonCloudWatch>();
            amazonCloudWatchMock.Setup(c => c.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricDataResponse());

            Mock<IAmazonCloudWatchLogs> amazonCloudWatchLogsMock = new Mock<IAmazonCloudWatchLogs>();
            amazonCloudWatchLogsMock.Setup(l => l.PutLogEventsAsync(It.IsAny<PutLogEventsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutLogEventsResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogGroupAsync(It.IsAny<CreateLogGroupRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogGroupResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogStreamAsync(It.IsAny<CreateLogStreamRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogStreamResponse());

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "Aws:BucketName", "bucket" }, { "Aws:QueueUrl", "queue" }, { "Aws:DeadLetterQueueUrl", "" } }!).Build();

            CloudWatchMonitoringService monitoring = new CloudWatchMonitoringService(amazonCloudWatchMock.Object, amazonCloudWatchLogsMock.Object, configuration);
            CsvProcessorService service = new CsvProcessorService(amazonS3Mock.Object, amazonSQSMock.Object, clientRepository.Object, configuration, monitoring);
            CsvMessageDTO message = new CsvMessageDTO { RequestId = Guid.NewGuid(), S3Key = "file.csv" };
            int imported = await service.ProcessAsync(message, CancellationToken.None);

            Assert.Equal(1, imported);
            Assert.NotNull(saved);
            Assert.Single(saved!);
            Assert.Equal("52998224725", saved![0].Cpf);
            clientRepository.Verify(x => x.AddRangeAsync(It.IsAny<List<Client>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_WithInvalidRecord_DoesNotImport()
        {
            string csv = "Name,Cpf,Email\r\n" +
                         "John Doe,12345678900,john@example.com\r\n" +
                         "\"Maria, Clara\",12345678900,maria.clara@example.com\r\n" +
                         "\"Ana \"\"Nina\"\" Souza\",12345678900,nina.souza@example.com\r\n" +
                         "José Ávila,12345678900,jose.avila@example.com";

            Mock<IAmazonS3> amazonS3Mock = new Mock<IAmazonS3>();
            amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() =>
                        {
                            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
                            return new GetObjectResponse { ResponseStream = memoryStream };
                        });

            Mock<IAmazonSQS> amazonSQSMock = new Mock<IAmazonSQS>();
            amazonSQSMock.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SendMessageResponse());

            Mock<IClientRepository> clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Mock<IAmazonCloudWatch> amazonCloudWatchMock = new Mock<IAmazonCloudWatch>();
            amazonCloudWatchMock.Setup(c => c.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricDataResponse());

            Mock<IAmazonCloudWatchLogs> amazonCloudWatchLogsMock = new Mock<IAmazonCloudWatchLogs>();
            amazonCloudWatchLogsMock.Setup(l => l.PutLogEventsAsync(It.IsAny<PutLogEventsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutLogEventsResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogGroupAsync(It.IsAny<CreateLogGroupRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogGroupResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogStreamAsync(It.IsAny<CreateLogStreamRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogStreamResponse());

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                                                                     {
                                                                         {"Aws:BucketName", "bucket"},
                                                                         {"Aws:QueueUrl", "queue"},
                                                                         {"Aws:DeadLetterQueueUrl", ""}
                                                                     }!).Build();

            CloudWatchMonitoringService monitoring = new CloudWatchMonitoringService(amazonCloudWatchMock.Object, amazonCloudWatchLogsMock.Object, configuration);
            CsvProcessorService service = new CsvProcessorService(amazonS3Mock.Object, amazonSQSMock.Object, clientRepository.Object, configuration, monitoring);

            CsvMessageDTO message = new CsvMessageDTO { RequestId = Guid.NewGuid(), S3Key = "file.csv" };

            int imported = await service.ProcessAsync(message, CancellationToken.None);

            Assert.Equal(0, imported);
            clientRepository.Verify(x => x.AddRangeAsync(It.IsAny<List<Client>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UploadToS3AndEnqueueAsync_UploadsAndEnqueues()
        {
            Mock<IAmazonS3> amazonS3Mock = new Mock<IAmazonS3>();
            PutObjectRequest? putRequest = null;
            amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>())).Callback<PutObjectRequest, CancellationToken>((req, token) => putRequest = req).ReturnsAsync(new PutObjectResponse());

            Mock<IAmazonSQS> amazonSQSMock = new Mock<IAmazonSQS>();
            SendMessageRequest? sentMessage = null;
            amazonSQSMock.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>())).Callback<SendMessageRequest, CancellationToken>((req, token) => sentMessage = req).ReturnsAsync(new SendMessageResponse());

            Mock<IClientRepository> clientRepository = new Mock<IClientRepository>();
            Mock<IAmazonCloudWatch> amazonCloudWatchMock = new Mock<IAmazonCloudWatch>();
            amazonCloudWatchMock.Setup(c => c.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutMetricDataResponse());

            Mock<IAmazonCloudWatchLogs> amazonCloudWatchLogsMock = new Mock<IAmazonCloudWatchLogs>();
            amazonCloudWatchLogsMock.Setup(l => l.PutLogEventsAsync(It.IsAny<PutLogEventsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PutLogEventsResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogGroupAsync(It.IsAny<CreateLogGroupRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogGroupResponse());
            amazonCloudWatchLogsMock.Setup(l => l.CreateLogStreamAsync(It.IsAny<CreateLogStreamRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLogStreamResponse());

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                                                                     {
                                                                         {"Aws:BucketName", "bucket"},
                                                                         {"Aws:QueueUrl", "queue"}
                                                                     }!).Build();

            CloudWatchMonitoringService monitoring = new CloudWatchMonitoringService(amazonCloudWatchMock.Object, amazonCloudWatchLogsMock.Object, configuration);
            CsvProcessorService service = new CsvProcessorService(amazonS3Mock.Object, amazonSQSMock.Object, clientRepository.Object, configuration, monitoring);

            string content = "Name,Cpf,Email\nJane Doe,52998224725,jane@example.com";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            IFormFile file = new FormFile(memoryStream, 0, memoryStream.Length, "file", "test.csv");

            Guid requestId = await service.UploadToS3AndEnqueueAsync(file, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, requestId);
            Assert.NotNull(putRequest);
            Assert.Equal("bucket", putRequest!.BucketName);
            Assert.Equal($"uploads/{requestId}.csv", putRequest.Key);

            Assert.NotNull(sentMessage);
            var payload = JsonSerializer.Deserialize<JsonElement>(sentMessage!.MessageBody);
            Assert.Equal(requestId.ToString(), payload.GetProperty("RequestId").GetString());
            Assert.Equal(putRequest.Key, payload.GetProperty("S3Key").GetString());

            amazonS3Mock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            amazonSQSMock.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}