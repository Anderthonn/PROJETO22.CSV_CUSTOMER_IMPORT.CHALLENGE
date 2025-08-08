using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Common.DTOs
{
    public class ClientDTOTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties()
        {
            string name = "John Doe";
            string cpf = "12345678900";
            string email = "john.doe@example.com";

            ClientDTO clientDTO = new ClientDTO(name, cpf, email);

            Assert.Equal(name, clientDTO.Name);
            Assert.Equal(cpf, clientDTO.Cpf);
            Assert.Equal(email, clientDTO.Email);
        }
    }
}