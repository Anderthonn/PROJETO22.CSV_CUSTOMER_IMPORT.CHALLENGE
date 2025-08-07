using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Repositories;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Services;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.IOC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConnectionStrings")));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<COMMON.Mappings.MappingProfile>();
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AddClientCommandHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(DeleteClientCommandHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(UpdateClientCommandHandler).Assembly);

                cfg.RegisterServicesFromAssembly(typeof(GetAllClientQuery).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GetByIdClientQuery).Assembly);
            });

            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ICsvProcessorService, CsvProcessorService>();

            return services;
        }
    }
}