using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces
{
    public interface IClientRepository
    {
        Task AddAsync(Client client, CancellationToken cancellationToken = default);
        Task AddRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default);
        Task DeleteAsync(Client account, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default);
        Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default);
        Task<List<Client>> GetAll(CancellationToken cancellationToken = default);
        Task<Client?> GetById(int id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default);
    }
}