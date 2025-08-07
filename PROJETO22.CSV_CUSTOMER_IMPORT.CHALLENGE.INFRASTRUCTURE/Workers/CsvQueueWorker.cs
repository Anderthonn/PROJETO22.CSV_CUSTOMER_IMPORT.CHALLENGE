using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Workers
{
    public class CsvQueueWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAmazonSQS _amazonSQS;
        private readonly string _queueUrl;

        public CsvQueueWorker(IServiceScopeFactory scopeFactory, IAmazonSQS amazonSQS, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _amazonSQS = amazonSQS;
            _queueUrl = configuration["Aws:QueueUrl"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
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

                    await processor.ProcessAsync(body, cancellationToken);

                    await _amazonSQS.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}