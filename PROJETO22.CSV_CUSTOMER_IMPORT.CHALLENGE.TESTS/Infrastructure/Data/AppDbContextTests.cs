using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Infrastructure.Data;

public class AppDbContextTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Client_entity_has_expected_configuration()
    {
        using AppDbContext context = CreateContext();

        IEntityType entity = context.Model.FindEntityType(typeof(Client))!;
        Assert.NotNull(entity);

        IKey key = entity!.FindPrimaryKey()!;
        Assert.NotNull(key);
        Assert.Single(key!.Properties);
        Assert.Equal("Id", key.Properties[0].Name);

        IProperty name = entity.FindProperty(nameof(Client.Name))!;
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(200, name.GetMaxLength());

        IProperty cpf = entity.FindProperty(nameof(Client.Cpf))!;
        Assert.NotNull(cpf);
        Assert.False(cpf!.IsNullable);
        Assert.Equal(11, cpf.GetMaxLength());

        IProperty email = entity.FindProperty(nameof(Client.Email))!;
        Assert.NotNull(email);
        Assert.False(email!.IsNullable);
        Assert.Equal(320, email.GetMaxLength());

        IProperty createdAt = entity.FindProperty(nameof(Client.CreatedAt))!;
        Assert.NotNull(createdAt);
        Assert.False(createdAt!.IsNullable);
    }
}