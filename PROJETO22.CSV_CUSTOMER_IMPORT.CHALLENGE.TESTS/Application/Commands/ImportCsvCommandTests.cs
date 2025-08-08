using Microsoft.AspNetCore.Http;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;
using System.Text;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class ImportCsvCommandTests
    {
        [Fact]
        public void Constructor_AssignsFile()
        {
            string csvContent = "name,email\nJohn,john@example.com";
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            FormFile formFile = new FormFile(stream, 0, stream.Length, "file", "test.csv");

            ImportCsvCommand command = new ImportCsvCommand(formFile);

            Assert.Equal(formFile, command.File);
        }
    }
}