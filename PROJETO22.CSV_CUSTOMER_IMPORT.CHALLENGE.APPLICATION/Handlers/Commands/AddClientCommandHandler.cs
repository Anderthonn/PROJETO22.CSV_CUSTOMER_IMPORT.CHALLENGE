using AutoMapper;
using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Validators;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public class AddClientCommandHandler : IRequestHandler<AddClientCommand, ClientDTO>
    {
        public readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public AddClientCommandHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<ClientDTO> Handle(AddClientCommand request, CancellationToken cancellationToken)
        {
            bool cpfValidator = CpfValidator.IsValid(request.Cpf);
            bool emailValidator = EmailValidator.IsValid(request.Email);

            if (cpfValidator == true && emailValidator == true)
            {
                bool? existsByCpf = await _clientRepository.ExistsByCpfAsync(request.Cpf);

                if (existsByCpf == true)
                    throw new Exception("Cpf already exists.");

                Client client = new Client(request.Name, request.Cpf, request.Email);
                await _clientRepository.AddAsync(client);

                return _mapper.Map<ClientDTO>(client);
            }
            else
            {
                throw new ArgumentException($"CPF: {request.Cpf} or Email: {request.Email} Invalid!");
            }
        }
    }
}
