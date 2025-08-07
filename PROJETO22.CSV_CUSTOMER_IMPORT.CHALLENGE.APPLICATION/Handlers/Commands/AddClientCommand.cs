using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public record AddClientCommand(string Name, string Cpf, string Email) : IRequest<ClientDTO>;
}