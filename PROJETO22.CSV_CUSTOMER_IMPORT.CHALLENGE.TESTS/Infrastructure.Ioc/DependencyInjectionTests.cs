using Amazon.CloudWatch;
using Amazon.CloudWatchLogs;
using Amazon.S3;
using Amazon.SQS;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.IOC;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Repositories;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Workers;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.IOC
{
    public class DependencyInjectionTests
    {
        private readonly ServiceCollection _services = new();

        public DependencyInjectionTests()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                              {
                                  { "ConnectionStrings:DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=Test;Trusted_Connection=True;" },
                                  { "AWS:Profile", "local" },
                                  { "AWS:Region", "us-east-1" }
                              }).Build();

            _services.AddLogging();

            _services.AddOptions();

            _services.AddInfrastructure(configuration);
        }

        [Fact]
        public void AddInfrastructure_Registers_AllExpectedServices()
        {
            Assert.Contains(_services, d => d.ServiceType == typeof(AppDbContext));
            Assert.Contains(_services, d => d.ServiceType == typeof(IAmazonS3));
            Assert.Contains(_services, d => d.ServiceType == typeof(IAmazonSQS));
            Assert.Contains(_services, d => d.ServiceType == typeof(IAmazonCloudWatch));
            Assert.Contains(_services, d => d.ServiceType == typeof(IAmazonCloudWatchLogs));
            Assert.Contains(_services, d => d.ServiceType == typeof(IClientRepository) && d.ImplementationType == typeof(ClientRepository));
            Assert.Contains(_services, d => d.ServiceType == typeof(CloudWatchMonitoringService));
            Assert.Contains(_services, d => d.ServiceType == typeof(ICsvProcessorService) && d.ImplementationType == typeof(CsvProcessorService));
            Assert.Contains(_services, d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(CsvQueueWorker));

            ServiceProvider provider = _services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<IMediator>());
            Assert.NotNull(provider.GetService<IMapper>());
        }
    }
}