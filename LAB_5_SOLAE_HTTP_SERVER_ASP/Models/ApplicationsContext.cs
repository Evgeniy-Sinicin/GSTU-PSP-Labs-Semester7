using Microsoft.EntityFrameworkCore;

namespace LAB_5_SOLAE_HTTP_SERVER_ASP.Models
{
    public class ApplicationsContext : DbContext
    {
        public ApplicationsContext(DbContextOptions<ApplicationsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Solae> Solaes { get; set; }
    }
}
