using MediatR;
using Microsoft.AspNetCore.Http;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public record ImportCsvCommand(IFormFile File) : IRequest<Guid>;
}