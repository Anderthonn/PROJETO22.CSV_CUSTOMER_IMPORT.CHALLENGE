using Microsoft.AspNetCore.Http;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Interfaces
{
    public interface ICsvProcessorService
    {
        Task<int> ProcessAsync(CsvMessageDTO message, CancellationToken cancellationToken);
        Task<Guid> UploadToS3AndEnqueueAsync(IFormFile file, CancellationToken cancellationToken);
    }
}