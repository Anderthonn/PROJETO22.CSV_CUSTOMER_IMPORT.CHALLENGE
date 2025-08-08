using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;
using System.Text.Json;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.DTOs
{
    public class CsvMessageDTOTests
    {
        [Fact]
        public void SerializeDeserialize_PreservesRequestIdAndS3Key()
        {
            Guid expectedRequestId = Guid.NewGuid();
            string expectedS3Key = "bucket/key.csv";
            CsvMessageDTO csvMessageDTO = new CsvMessageDTO
            {
                RequestId = expectedRequestId,
                S3Key = expectedS3Key
            };

            string json = JsonSerializer.Serialize(csvMessageDTO);
            CsvMessageDTO result = JsonSerializer.Deserialize<CsvMessageDTO>(json)!;

            Assert.NotNull(result);
            Assert.Equal(expectedRequestId, result!.RequestId);
            Assert.Equal(expectedS3Key, result.S3Key);
        }
    }
}