using AutoMapper;
using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDTO>
    {
        public readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public UpdateClientCommandHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<ClientDTO> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            Client? client = await _clientRepository.GetById(request.Id, cancellationToken) ?? null;

            bool cpfValidator = CpfValidator.IsValid(request.Cpf!);
            bool emailValidator = EmailValidator.IsValid(request.Email!);

            if (cpfValidator == true && emailValidator == true)
            {
                if (client == null)
                    throw new KeyNotFoundException($"Client: {request.Id} not found!");

                client = new Client(request.Name!, request.Cpf!, request.Email!);

                await _clientRepository.UpdateAsync(client, cancellationToken);

                return _mapper.Map<ClientDTO>(client);
            }
            else
            {
                throw new ArgumentException($"CPF: {request.Cpf} or Email: {request.Email} Invalid!");
            }
        }
    }
}