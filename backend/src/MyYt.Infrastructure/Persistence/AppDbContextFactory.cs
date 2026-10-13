using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyYt.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef</c> build the model without a .env file or a reachable server.</summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseMySQL("Server=localhost;Database=myyt;User=design;Password=design")
            .Options);
}
