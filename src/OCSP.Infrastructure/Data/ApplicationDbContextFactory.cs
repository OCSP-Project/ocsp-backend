using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OCSP.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // ✅ Connection string trùng với .env
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=ocsp;Username=ocsp;Password=ocsp"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
