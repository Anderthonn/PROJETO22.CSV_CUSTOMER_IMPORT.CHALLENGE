using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public record UpdateClientCommand(int Id, string? Name = null, string? Cpf = null, string? Email = null) : IRequest<ClientDTO>;
}
