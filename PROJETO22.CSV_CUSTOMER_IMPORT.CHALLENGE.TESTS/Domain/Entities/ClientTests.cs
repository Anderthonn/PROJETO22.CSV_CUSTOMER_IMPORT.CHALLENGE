using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Domain.Entities
{
    public class ClientTests
    {
        [Fact]
        public void Constructor_Should_Set_Properties()
        {
            string name = "John Doe";
            string cpf = "12345678900";
            string email = "john.doe@example.com";
            DateTime before = DateTime.UtcNow;

            Client client = new Client(name, cpf, email);
            DateTime after = DateTime.UtcNow;

            Assert.Equal(name, client.Name);
            Assert.Equal(cpf, client.Cpf);
            Assert.Equal(email, client.Email);
            Assert.True(client.CreatedAt >= before && client.CreatedAt <= after);
        }

        [Theory]
        [InlineData(null, "12345678900", "john.doe@example.com", nameof(Client.Name))]
        [InlineData("John Doe", null, "john.doe@example.com", nameof(Client.Cpf))]
        [InlineData("John Doe", "12345678900", null, nameof(Client.Email))]
        public void Constructor_Should_Throw_ArgumentNullException_When_Arguments_Are_Null(string? name, string? cpf, string? email, string paramName)
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Client(name!, cpf!, email!));
            Assert.Equal(paramName, exception.ParamName, StringComparer.OrdinalIgnoreCase);
        }
    }
}