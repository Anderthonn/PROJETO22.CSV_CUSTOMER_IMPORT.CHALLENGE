using AutoMapper;
using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries
{
    public class GetByIdClientQueryHandler : IRequestHandler<GetByIdClientQuery, ClientDTO>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public GetByIdClientQueryHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<ClientDTO> Handle(GetByIdClientQuery request, CancellationToken cancellationToken)
        {
            Client? client = await _clientRepository.GetById(request.Id, cancellationToken);

            if (client == null)
                throw new KeyNotFoundException($"Client {request.Id} not found.");

            return _mapper.Map<ClientDTO>(client);
        }
    }
}