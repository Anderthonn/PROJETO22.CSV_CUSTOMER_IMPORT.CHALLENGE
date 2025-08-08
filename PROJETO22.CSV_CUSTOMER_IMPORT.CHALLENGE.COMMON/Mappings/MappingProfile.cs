using AutoMapper;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ClientDTO, Client>().ConstructUsing(clientDTO => new Client(clientDTO.Name, clientDTO.Cpf, clientDTO.Email)).ForMember(d => d.Id, opt => opt.Ignore()).ForMember(d => d.CreatedAt, opt => opt.Ignore());
        }
    }
}