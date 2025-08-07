using Amazon.CloudWatch;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using System.Globalization;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services
{
    public class CsvProcessorService : ICsvProcessorService
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly IAmazonSQS _amazonSQS;
        private readonly string _bucketName;
        private readonly IClientRepository _clientRepository;
        private readonly string _queueUrl;
        private readonly CloudWatchMonitoringService _monitoring;

        public CsvProcessorService(IAmazonS3 amazonS3, IAmazonSQS amazonSQS, IClientRepository clientRepository, IConfiguration configuration, CloudWatchMonitoringService monitoring)
        {
            _amazonS3 = amazonS3;
            _amazonSQS = amazonSQS;
            _bucketName = configuration["Aws:BucketName"]!;
            _clientRepository = clientRepository;
            _queueUrl = configuration["Aws:QueueUrl"]!;
            _monitoring = monitoring;
        }

        public async Task<int> ProcessAsync(CsvMessageDTO message, CancellationToken cancellationToken)
        {
            DateTime start = DateTime.UtcNow;
            int imported = 0;
            await _monitoring.LogAsync("CsvProcessorService", $"Processing file {message.S3Key}", cancellationToken);

            try
            {
                GetObjectRequest getReq = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = message.S3Key
                };
                using GetObjectResponse getResp = await _amazonS3.GetObjectAsync(getReq, cancellationToken);
                using Stream stream = getResp.ResponseStream;
                using StreamReader reader = new StreamReader(stream);

                CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    TrimOptions = TrimOptions.Trim,
                    IgnoreBlankLines = true
                };
                using CsvReader csv = new CsvReader(reader, config);

                const int batchSize = 500;
                List<Client> batch = new List<Client>(batchSize);

                await foreach (ClientDTO record in csv.GetRecordsAsync<ClientDTO>().WithCancellation(cancellationToken))
                {
                    try
                    {
                        if (!await _clientRepository.ExistsByCpfAsync(record.Cpf, cancellationToken))
                        {
                            Client client = new Client(record.Name, record.Cpf, record.Email);
                            batch.Add(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _monitoring.LogAsync("CsvProcessorService", $"Record error: {ex.Message}", cancellationToken);
                        await _monitoring.PublishMetricAsync("Failures", 1);
                    }

                    if (batch.Count >= batchSize)
                    {
                        await _clientRepository.AddRangeAsync(batch, cancellationToken);
                        imported += batch.Count;
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await _clientRepository.AddRangeAsync(batch, cancellationToken);
                    imported += batch.Count;
                }
            }
            catch (Exception ex)
            {
                await _monitoring.LogAsync("CsvProcessorService", $"Critical failure: {ex.Message}", cancellationToken);
                await _monitoring.PublishMetricAsync("CriticalExceptions", 1);
                await _monitoring.PublishMetricAsync("Failures", 1);
                throw;
            }
            finally
            {
                double elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                await _monitoring.PublishMetricAsync("ProcessingTime", elapsed, StandardUnit.Milliseconds);
                await _monitoring.PublishMetricAsync("RecordsImported", imported);
                await _monitoring.LogAsync("CsvProcessorService", $"Processed {imported} records in {elapsed} ms", cancellationToken);
            }

            return imported;
        }

        public async Task<Guid> UploadToS3AndEnqueueAsync(IFormFile file, CancellationToken cancellationToken)
        {
            Guid requestId = Guid.NewGuid();
            string key = $"uploads/{requestId}.csv";
            await _monitoring.LogAsync("CsvProcessorService", $"Uploading {key}", cancellationToken);

            using Stream stream = file.OpenReadStream();
            PutObjectRequest putObjectRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream
            };
            await _amazonS3.PutObjectAsync(putObjectRequest);

            await _amazonSQS.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = JsonSerializer.Serialize(new { RequestId = requestId, S3Key = key })
            });

            await _monitoring.LogAsync("CsvProcessorService", $"Enqueued request {requestId}", cancellationToken);
            return requestId;
        }
    }
}