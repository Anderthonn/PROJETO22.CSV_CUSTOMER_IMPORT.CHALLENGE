using AutoMapper;
using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries
{
    public class GetAllClientQueryHandler : IRequestHandler<GetAllClientQuery, List<ClientDTO>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public GetAllClientQueryHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<List<ClientDTO>> Handle(GetAllClientQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<List<ClientDTO>>(await _clientRepository.GetAll(cancellationToken));
        }
    }
}