using Amazon.CloudWatch;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Workers
{
    public class CsvQueueWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAmazonSQS _amazonSQS;
        private readonly string _queueUrl;
        private readonly string _deadLetterQueueUrl;
        private readonly CloudWatchMonitoringService _monitoring;
        private readonly string _degreeOfParallelism;

        public CsvQueueWorker(IServiceScopeFactory scopeFactory, IAmazonSQS amazonSQS, IConfiguration configuration, CloudWatchMonitoringService monitoring)
        {
            _scopeFactory = scopeFactory;
            _amazonSQS = amazonSQS;
            _queueUrl = configuration["Aws:QueueUrl"]!;
            _deadLetterQueueUrl = configuration["Aws:DeadLetterQueueUrl"] ?? string.Empty;
            _monitoring = monitoring;
            _degreeOfParallelism = configuration.GetSection("CsvQueueWorker")["DegreeOfParallelism"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
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

                    await Parallel.ForEachAsync(response.Messages,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = int.Parse(_degreeOfParallelism),
                        CancellationToken = cancellationToken
                    },
                    async (message, token) =>
                    {
                        CsvMessageDTO body = JsonSerializer.Deserialize<CsvMessageDTO>(message.Body)!;

                        using IServiceScope scope = _scopeFactory.CreateScope();
                        ICsvProcessorService processor = scope.ServiceProvider.GetRequiredService<ICsvProcessorService>();

                        DateTime start = DateTime.UtcNow;
                        try
                        {
                            await _monitoring.LogAsync("CsvQueueWorker", $"Processing message {message.MessageId}", token);
                            int imported = await processor.ProcessAsync(body, token);
                            await _monitoring.PublishMetricAsync("RecordsImported", imported);
                            await _amazonSQS.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, token);
                            await _monitoring.LogAsync("CsvQueueWorker", $"Processed message {message.MessageId} importing {imported} records", token);
                        }
                        catch (Exception ex)
                        {
                            await _monitoring.LogAsync("CsvQueueWorker", $"Error processing message {message.MessageId}: {ex.Message}", token);
                            await _monitoring.PublishMetricAsync("Failures", 1);
                            await _monitoring.PublishMetricAsync("CriticalExceptions", 1);
                        }
                        finally
                        {
                            double elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                            await _monitoring.PublishMetricAsync("ProcessingTime", elapsed, StandardUnit.Milliseconds);
                        }
                    });

                    if (!string.IsNullOrEmpty(_deadLetterQueueUrl))
                    {
                        ReceiveMessageResponse dlqResponse = await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
                        {
                            QueueUrl = _deadLetterQueueUrl,
                            MaxNumberOfMessages = 5
                        }, cancellationToken);

                        foreach (Message dlqMessage in dlqResponse.Messages)
                        {
                            ClientDTO record = JsonSerializer.Deserialize<ClientDTO>(dlqMessage.Body)!;

                            using IServiceScope scope = _scopeFactory.CreateScope();
                            IClientRepository repository = scope.ServiceProvider.GetRequiredService<IClientRepository>();

                            try
                            {
                                if (!await repository.ExistsByCpfAsync(record.Cpf, cancellationToken))
                                {
                                    Client client = new Client(record.Name, record.Cpf, record.Email);
                                    await repository.AddAsync(client, cancellationToken);
                                    await _monitoring.PublishReprocessedRecordsMetricAsync(1);
                                    await _amazonSQS.DeleteMessageAsync(_deadLetterQueueUrl, dlqMessage.ReceiptHandle, cancellationToken);
                                    await _monitoring.LogAsync("CsvQueueWorker", $"Reprocessed record {record.Cpf}", cancellationToken);
                                }
                                else
                                {
                                    await _amazonSQS.DeleteMessageAsync(_deadLetterQueueUrl, dlqMessage.ReceiptHandle, cancellationToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                await _monitoring.LogAsync("CsvQueueWorker", $"Error reprocessing record {record.Cpf}: {ex.Message}", cancellationToken);
                                await _monitoring.PublishMetricAsync("Failures", 1);
                            }
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (Exception) { }
        }
    }
}