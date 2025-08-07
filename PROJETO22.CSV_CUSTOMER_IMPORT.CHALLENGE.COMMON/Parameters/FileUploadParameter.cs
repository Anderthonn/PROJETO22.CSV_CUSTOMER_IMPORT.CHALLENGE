using Microsoft.AspNetCore.Http;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Parameters
{
    public class FileUploadParameter
    {
        public IFormFile File { get; set; } = null!;
    }
}