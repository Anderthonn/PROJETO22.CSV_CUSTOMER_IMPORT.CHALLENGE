namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities
{
    public class Client
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private Client() { }

        public Client(string name, string cpf, string email)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Cpf = cpf ?? throw new ArgumentNullException(nameof(cpf));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CreatedAt = DateTime.UtcNow;
        }
    }
}