using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace cmsUserManagment.Infrastructure.Persistance;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Reads from env var first (set this when running migrations against Docker Compose),
        // otherwise falls back to the local Docker Compose default.
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=33063;Database=auth;Uid=user;Pwd=alskjdfa@alskdjfAAAb12;";

        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
        return new AppDbContext(optionsBuilder.Options);
    }
}
