using MediatR;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public record DeleteClientCommand(int Id) : IRequest<Unit>;
}