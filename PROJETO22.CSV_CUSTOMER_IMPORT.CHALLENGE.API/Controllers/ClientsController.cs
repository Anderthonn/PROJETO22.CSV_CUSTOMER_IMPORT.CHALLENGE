using MediatR;
using Microsoft.AspNetCore.Mvc;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Parameters;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.API.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("importar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] FileUploadParameter fileUploadRequest)
        {
            IFormFile file = fileUploadRequest.File;

            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required.");

            Guid requestId = await _mediator.Send(new ImportCsvCommand(file));
            return Accepted(new { RequestId = requestId });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            await _mediator.Send(new DeleteClientCommand(id));
            return Ok(new { message = "Client removed successfully." });
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _mediator.Send(new GetAllClientQuery()));
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetClientById([FromQuery] int id)
        {
            return Ok(await _mediator.Send(new GetByIdClientQuery(id)));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AddClientCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Client registered successfully." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateClientCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Client update successfully." });
        }
    }
}