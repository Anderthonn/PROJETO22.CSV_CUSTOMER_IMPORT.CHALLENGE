using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Mappings;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.Mappings
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfile>(); }, NullLoggerFactory.Instance);
            configuration.AssertConfigurationIsValid();
            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void Should_Map_ClientDTO_To_Client()
        {
            ClientDTO clientDTO = new ClientDTO("John Doe", "12345678901", "john@example.com");
            Client entity = _mapper.Map<Client>(clientDTO);

            Assert.NotNull(entity);
            Assert.Equal(clientDTO.Name, entity.Name);
            Assert.Equal(clientDTO.Cpf, entity.Cpf);
            Assert.Equal(clientDTO.Email, entity.Email);
        }
    }
}