using Microsoft.AspNetCore.Http;
using Moq;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class ImportCsvCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldUploadFileAndReturnRequestId()
        {
            IFormFile file = new Mock<IFormFile>().Object;
            Guid expectedId = Guid.NewGuid();
            Mock<ICsvProcessorService> processorMock = new Mock<ICsvProcessorService>();

            processorMock.Setup(p => p.UploadToS3AndEnqueueAsync(file, It.IsAny<CancellationToken>())).ReturnsAsync(expectedId);

            ImportCsvCommandHandler handler = new ImportCsvCommandHandler(processorMock.Object);
            ImportCsvCommand command = new ImportCsvCommand(file);

            Guid result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(expectedId, result);
            processorMock.Verify(p => p.UploadToS3AndEnqueueAsync(file, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}