using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Application.Commands
{
    public class AddClientCommandTests
    {
        [Fact]
        public void Constructor_ShouldStoreProperties()
        {
            const string name = "John Doe";
            const string cpf = "12345678901";
            const string email = "john@example.com";

            AddClientCommand command = new AddClientCommand(name, cpf, email);

            Assert.Equal(name, command.Name);
            Assert.Equal(cpf, command.Cpf);
            Assert.Equal(email, command.Email);
        }
    }
}