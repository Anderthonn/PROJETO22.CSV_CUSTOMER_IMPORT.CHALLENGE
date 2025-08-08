using AutoMapper;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Queries
{
    public class GetByIdClientQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetByIdClientQueryHandler _handler;

        public GetByIdClientQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetByIdClientQueryHandler(_clientRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsClientDto_WhenClientExists()
        {
            Client client = new Client("John Doe", "12345678900", "john@example.com");
            ClientDTO clientDTO = new ClientDTO("John Doe", "12345678900", "john@example.com");

            _clientRepositoryMock.Setup(r => r.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _mapperMock.Setup(m => m.Map<ClientDTO>(client)).Returns(clientDTO);

            GetByIdClientQuery query = new GetByIdClientQuery(1);

            ClientDTO result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(clientDTO, result);
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenClientNotFound()
        {
            _clientRepositoryMock.Setup(r => r.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

            GetByIdClientQuery query = new GetByIdClientQuery(1);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
            _mapperMock.Verify(m => m.Map<ClientDTO>(It.IsAny<Client>()), Times.Never);
        }
    }
}