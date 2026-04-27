using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nutq.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // نفس الـ connection string اللي هتستخدميه في Web
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=NutqDB;Username=postgres;Password=1234");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
