using MediatR;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class DeleteClientCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteClient_WhenClientExists()
        {
            Mock<IClientRepository> repositoryMock = new Mock<IClientRepository>();
            DeleteClientCommandHandler handler = new DeleteClientCommandHandler(repositoryMock.Object);
            Client client = new Client("Name", "12345678901", "email@example.com");

            repositoryMock.Setup(r => r.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            repositoryMock.Setup(r => r.DeleteAsync(client, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            DeleteClientCommand command = new DeleteClientCommand(1);

            Unit result = await handler.Handle(command, CancellationToken.None);

            repositoryMock.Verify(r => r.DeleteAsync(client, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenClientNotFound()
        {
            Mock<IClientRepository> repositoryMock = new Mock<IClientRepository>();
            repositoryMock.Setup(r => r.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

            DeleteClientCommandHandler handler = new DeleteClientCommandHandler(repositoryMock.Object);
            DeleteClientCommand command = new DeleteClientCommand(1);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
            repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}