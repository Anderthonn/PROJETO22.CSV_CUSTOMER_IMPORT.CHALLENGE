namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.COMMON.DTOs
{
    public class ClientDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ClientDTO() { }

        public ClientDTO(string name, string cpf, string email)
        {
            Name = name;
            Cpf = cpf;
            Email = email;
        }
    }
}