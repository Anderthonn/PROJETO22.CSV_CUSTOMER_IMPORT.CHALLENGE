using AutoMapper;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class UpdateClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateClientCommandHandler _handler;

        public UpdateClientCommandHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateClientCommandHandler(_clientRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateClient()
        {
            UpdateClientCommand command = new UpdateClientCommand(1, "John Doe", "52998224725", "john@example.com");
            Client existingClient = new Client("Old Name", "52998224725", "old@example.com");

            _clientRepositoryMock.Setup(repo => repo.GetById(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingClient);

            _clientRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<ClientDTO>(It.IsAny<Client>())).Returns((Client client) => new ClientDTO(client.Name, client.Cpf, client.Email));

            ClientDTO result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(command.Name, result.Name);
            Assert.Equal(command.Cpf, result.Cpf);
            Assert.Equal(command.Email, result.Email);
            _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.Name == command.Name && c.Cpf == command.Cpf && c.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("invalidcpf", "john@example.com")]
        [InlineData("52998224725", "invalid-email")]
        public async Task Handle_InvalidCpfOrEmail_ShouldThrowArgumentException(string cpf, string email)
        {
            UpdateClientCommand command = new UpdateClientCommand(1, "John Doe", cpf, email);
            Client existingClient = new Client("Old Name", "52998224725", "old@example.com");

            _clientRepositoryMock.Setup(repo => repo.GetById(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingClient);

            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ClientNotFound_ShouldThrowKeyNotFoundException()
        {
            UpdateClientCommand command = new UpdateClientCommand(1, "John Doe", "52998224725", "john@example.com");

            _clientRepositoryMock.Setup(repo => repo.GetById(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}