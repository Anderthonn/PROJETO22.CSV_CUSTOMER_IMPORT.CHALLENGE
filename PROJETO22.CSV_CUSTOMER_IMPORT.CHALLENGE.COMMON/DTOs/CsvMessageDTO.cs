namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs
{
    public class CsvMessageDTO
    {
        public Guid RequestId { get; set; }
        public string S3Key { get; set; } = string.Empty;
    }
}