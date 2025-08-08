using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.API.Controllers;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Parameters;
using System.Text;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.API.Controllers;

public class ClientsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ClientsController _controller;

    public ClientsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ClientsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Import_ReturnsAcceptedAndSendsCommand()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ImportCsvCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));

        FileUploadParameter file = new FileUploadParameter
        {
            File = new FormFile(stream, 0, stream.Length, "file", "clients.csv")
        };

        IActionResult result = await _controller.Import(file);

        Assert.IsType<AcceptedResult>(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ImportCsvCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Import_WithNullFile_ReturnsBadRequest()
    {
        IActionResult result = await _controller.Import(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ImportCsvCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsOkAndSendsCommand()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteClientCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Unit.Value);

        IActionResult result = await _controller.Delete(1);

        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteClientCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndSendsQuery()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllClientQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ClientDTO>());

        IActionResult result = await _controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllClientQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetClientById_ReturnsOkAndSendsQuery()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetByIdClientQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ClientDTO("name", "cpf", "email"));

        IActionResult result = await _controller.GetClientById(1);

        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetByIdClientQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsOkAndSendsCommand()
    {
        AddClientCommand command = new AddClientCommand("name", "cpf", "email");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(new ClientDTO("name", "cpf", "email"));

        IActionResult result = await _controller.Register(command);

        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOkAndSendsCommand()
    {
        UpdateClientCommand command = new UpdateClientCommand(1, "name", "cpf", "email");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(new ClientDTO("name", "cpf", "email"));

        IActionResult result = await _controller.Update(command);

        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}