using AutoMapper;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Queries
{
    public class GetAllClientQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnMappedClients()
        {
            List<Client> clients = new List<Client>
            {
                new Client("John Doe", "12345678900", "john@example.com"),
                new Client("Jane Doe", "98765432100", "jane@example.com")
            };

            List<ClientDTO> expectedDtos = new List<ClientDTO>
            {
                new ClientDTO("John Doe", "12345678900", "john@example.com"),
                new ClientDTO("Jane Doe", "98765432100", "jane@example.com")
            };

            Mock<IClientRepository> repositoryMock = new Mock<IClientRepository>();
            repositoryMock.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(clients);

            Mock<IMapper> mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<List<ClientDTO>>(clients)).Returns(expectedDtos);

            GetAllClientQueryHandler handler = new GetAllClientQueryHandler(repositoryMock.Object, mapperMock.Object);

            List<ClientDTO> result = await handler.Handle(new GetAllClientQuery(), CancellationToken.None);

            Assert.Equal(expectedDtos.Count, result.Count);
            for (int i = 0; i < expectedDtos.Count; i++)
            {
                Assert.Equal(expectedDtos[i].Name, result[i].Name);
                Assert.Equal(expectedDtos[i].Cpf, result[i].Cpf);
                Assert.Equal(expectedDtos[i].Email, result[i].Email);
            }

            repositoryMock.Verify(r => r.GetAll(It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<ClientDTO>>(clients), Times.Once);
        }
    }
}