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

        public CsvProcessorService(IAmazonS3 amazonS3, IAmazonSQS amazonSQS, IClientRepository clientRepository, IConfiguration configuration)
        {
            _amazonS3 = amazonS3;
            _amazonSQS = amazonSQS;
            _bucketName = configuration["Aws:BucketName"]!;
            _clientRepository = clientRepository;
            _queueUrl = configuration["Aws:QueueUrl"]!;
        }

        public async Task ProcessAsync(CsvMessageDTO message, CancellationToken cancellationToken)
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
                catch (Exception)
                {
                }

                if (batch.Count >= batchSize)
                {
                    await _clientRepository.AddRangeAsync(batch, cancellationToken);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await _clientRepository.AddRangeAsync(batch, cancellationToken);
            }
        }

        public async Task<Guid> UploadToS3AndEnqueueAsync(IFormFile file)
        {
            Guid requestId = Guid.NewGuid();
            string key = $"uploads/{requestId}.csv";

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

            return requestId;
        }
    }
}