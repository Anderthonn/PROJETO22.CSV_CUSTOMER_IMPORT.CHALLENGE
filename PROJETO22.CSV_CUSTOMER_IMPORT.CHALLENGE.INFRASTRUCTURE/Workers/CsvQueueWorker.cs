using Amazon.CloudWatch;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Workers
{
    public class CsvQueueWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAmazonSQS _amazonSQS;
        private readonly string _queueUrl;
        private readonly CloudWatchMonitoringService _monitoring;

        public CsvQueueWorker(IServiceScopeFactory scopeFactory, IAmazonSQS amazonSQS, IConfiguration configuration, CloudWatchMonitoringService monitoring)
        {
            _scopeFactory = scopeFactory;
            _amazonSQS = amazonSQS;
            _queueUrl = configuration["Aws:QueueUrl"]!;
            _monitoring = monitoring;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string queueName = _queueUrl.Split('/').Last();
            await _monitoring.CreateFailureAlarmAsync("ConsecutiveFailures");
            await _monitoring.CreateQueueBacklogAlarmAsync("QueueBacklog", queueName, 10);
            await _monitoring.CreateExceptionAlarmAsync("CriticalExceptions");

            while (!cancellationToken.IsCancellationRequested)
            {
                ReceiveMessageResponse response = await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 5
                }, cancellationToken);

                foreach (Message message in response.Messages)
                {
                    CsvMessageDTO body = JsonSerializer.Deserialize<CsvMessageDTO>(message.Body)!;

                    using IServiceScope scope = _scopeFactory.CreateScope();
                    ICsvProcessorService processor = scope.ServiceProvider.GetRequiredService<ICsvProcessorService>();

                    DateTime start = DateTime.UtcNow;

                    try
                    {
                        await _monitoring.LogAsync("CsvQueueWorker", $"Processing message {message.MessageId}", cancellationToken);
                        int imported = await processor.ProcessAsync(body, cancellationToken);
                        await _monitoring.PublishMetricAsync("RecordsImported", imported);
                        await _amazonSQS.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                        await _monitoring.LogAsync("CsvQueueWorker", $"Processed message {message.MessageId} importing {imported} records", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await _monitoring.LogAsync("CsvQueueWorker", $"Error processing message {message.MessageId}: {ex.Message}", cancellationToken);
                        await _monitoring.PublishMetricAsync("Failures", 1);
                        await _monitoring.PublishMetricAsync("CriticalExceptions", 1);
                    }
                    finally
                    {
                        double elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                        await _monitoring.PublishMetricAsync("ProcessingTime", elapsed, StandardUnit.Milliseconds);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}