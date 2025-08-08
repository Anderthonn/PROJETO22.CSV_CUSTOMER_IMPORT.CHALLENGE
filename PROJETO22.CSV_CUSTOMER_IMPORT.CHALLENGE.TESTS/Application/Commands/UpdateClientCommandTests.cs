using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Handlers.Commands
{
    public class UpdateClientCommandTests
    {
        [Fact]
        public void Constructor_ShouldStoreId()
        {
            UpdateClientCommand command = new UpdateClientCommand(1);
            Assert.Equal(1, command.Id);
        }

        [Fact]
        public void Constructor_ShouldStoreProvidedName()
        {
            UpdateClientCommand command = new UpdateClientCommand(1, Name: "Jane Doe");
            Assert.Equal("Jane Doe", command.Name);
        }

        [Fact]
        public void Constructor_ShouldStoreProvidedCpf()
        {
            UpdateClientCommand command = new UpdateClientCommand(1, Cpf: "12345678900");
            Assert.Equal("12345678900", command.Cpf);
        }

        [Fact]
        public void Constructor_ShouldStoreProvidedEmail()
        {
            UpdateClientCommand command = new UpdateClientCommand(1, Email: "jane@example.com");
            Assert.Equal("jane@example.com", command.Email);
        }
    }
}