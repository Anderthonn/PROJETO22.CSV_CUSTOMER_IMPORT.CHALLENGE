using MediatR;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands
{
    public class ImportCsvCommandHandler : IRequestHandler<ImportCsvCommand, Guid>
    {
        private readonly ICsvProcessorService _processor;

        public ImportCsvCommandHandler(ICsvProcessorService processor)
        {
            _processor = processor;
        }

        public async Task<Guid> Handle(ImportCsvCommand request, CancellationToken cancellationToken)
        {
            return await _processor.UploadToS3AndEnqueueAsync(request.File);
        }
    }
}