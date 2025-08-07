using Microsoft.EntityFrameworkCore;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Interfaces;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            await _context.Clients.AddAsync(client, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default)
        {
            await _context.Clients.AddRangeAsync(listClients, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Client account, CancellationToken cancellationToken = default)
        {
            _context.Clients.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default)
        {
            _context.Clients.RemoveRange(listClients);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            return await _context.Clients.AsNoTracking().AnyAsync(c => c.Cpf == cpf, cancellationToken);
        }

        public async Task<List<Client>> GetAll(CancellationToken cancellationToken = default)
        {
            return await _context.Clients.ToListAsync(cancellationToken);
        }

        public async Task<Client?> GetById(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRangeAsync(List<Client> listClients, CancellationToken cancellationToken = default)
        {
            _context.Clients.UpdateRange(listClients);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}