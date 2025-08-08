using Microsoft.AspNetCore.Http;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.Parameters;
using System.Text;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.Parameters
{
    public class FileUploadParameterTests
    {
        [Fact]
        public void Should_hold_assigned_IFormFile_and_preserve_metadata_and_content()
        {
            string csv = "Nome,CPF,E-mail\nAna,11122233344,ana@exemplo.com\n";
            byte[] bytes = Encoding.UTF8.GetBytes(csv);
            MemoryStream stream = new MemoryStream(bytes);
            IFormFile formFile = new FormFile(stream, 0, bytes.Length, "file", "clientes.csv");

            FileUploadParameter param = new FileUploadParameter();

            param.File = formFile;

            Assert.NotNull(param.File);
            Assert.Equal("clientes.csv", param.File.FileName);
            Assert.Equal(bytes.Length, param.File.Length);

            using Stream read = param.File.OpenReadStream();
            using StreamReader streamReader = new StreamReader(read, Encoding.UTF8, leaveOpen: false);
            string content = streamReader.ReadToEnd();
            Assert.Equal(csv, content);
        }
    }
}