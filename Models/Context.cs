using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SipahiDomainCore.Models
{
    public class Context:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("ConnectionStrings"));
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<IPAddress> IPAddresses { get; set; }
    }
}
