using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Infrastructure.Persistence;

public class AppDesignDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=taskflow;User Id=sa;Password=Taskflow/26;TrustServerCertificate=true;");

        return new AppDbContext(optionsBuilder.Options);
    }
}