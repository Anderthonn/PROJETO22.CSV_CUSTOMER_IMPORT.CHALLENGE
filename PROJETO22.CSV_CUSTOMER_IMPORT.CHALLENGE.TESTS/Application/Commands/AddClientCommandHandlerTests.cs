using AutoMapper;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class AddClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        [Fact]
        public async Task Handle_ShouldAddClient_WhenCpfAndEmailAreValid()
        {
            AddClientCommand command = new AddClientCommand("John Doe", "52998224725", "john.doe@example.com");
            _clientRepositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<ClientDTO>(It.IsAny<Client>())).Returns((Client client) => new ClientDTO(client.Name, client.Cpf, client.Email));
            AddClientCommandHandler handler = new AddClientCommandHandler(_clientRepositoryMock.Object, _mapperMock.Object);

            ClientDTO result = await handler.Handle(command, CancellationToken.None);

            _clientRepositoryMock.Verify(r => r.AddAsync(It.Is<Client>(c => c.Name == command.Name && c.Cpf == command.Cpf && c.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(command.Name, result.Name);
            Assert.Equal(command.Cpf, result.Cpf);
            Assert.Equal(command.Email, result.Email);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCpfAlreadyExists()
        {
            AddClientCommand command = new AddClientCommand("Jane Doe", "52998224725", "jane.doe@example.com");
            _clientRepositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(true);
            AddClientCommandHandler handler = new AddClientCommandHandler(_clientRepositoryMock.Object, _mapperMock.Object);

            Exception ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Cpf already exists.", ex.Message);
            _clientRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}