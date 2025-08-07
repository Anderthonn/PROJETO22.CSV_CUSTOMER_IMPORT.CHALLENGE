using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries
{
    public record GetByIdClientQuery(int Id) : IRequest<ClientDTO>;
}