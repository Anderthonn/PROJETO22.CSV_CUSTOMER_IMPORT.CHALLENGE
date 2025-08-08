using Microsoft.EntityFrameworkCore;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Repositories;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.Repositories;

public class ClientRepositoryTests
{
    private static Client CreateClient(string name = "John Doe", string cpf = "12345678901", string email = "john@example.com") => new(name, cpf, email);

    private static ClientRepository CreateRepository(out AppDbContext context)
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        context = new AppDbContext(options);
        return new ClientRepository(context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistClient()
    {
        ClientRepository repository = CreateRepository(out var context);
        Client client = CreateClient();

        await repository.AddAsync(client);

        bool exists = await context.Clients.AnyAsync(c => c.Id == client.Id);
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnTrue_WhenCpfExists()
    {
        ClientRepository repository = CreateRepository(out _);
        Client client = CreateClient();
        await repository.AddAsync(client);

        bool exists = await repository.ExistsByCpfAsync(client.Cpf);

        Assert.True(exists);
    }

    [Fact]
    public async Task GetById_ShouldReturnClient_WhenExists()
    {
        ClientRepository repository = CreateRepository(out _);
        Client client = CreateClient();
        await repository.AddAsync(client);

        Client result = await repository.GetById(client.Id) ?? new Client();

        Assert.NotNull(result);
        Assert.Equal(client.Cpf, result!.Cpf);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingClient()
    {
        ClientRepository repository = CreateRepository(out _);
        Client client = CreateClient();
        await repository.AddAsync(client);

        typeof(Client).GetProperty("Name")!.SetValue(client, "Jane Doe");
        typeof(Client).GetProperty("Email")!.SetValue(client, "jane@example.com");

        await repository.UpdateAsync(client);

        Client updated = await repository.GetById(client.Id) ?? new Client();

        Assert.Equal("Jane Doe", updated!.Name);
        Assert.Equal("jane@example.com", updated.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveClient()
    {
        ClientRepository repository = CreateRepository(out _);
        Client client = CreateClient();
        await repository.AddAsync(client);

        await repository.DeleteAsync(client);

        var deleted = await repository.GetById(client.Id);
        Assert.Null(deleted);
    }
}