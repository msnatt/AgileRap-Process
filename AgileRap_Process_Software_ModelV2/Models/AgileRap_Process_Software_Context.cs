using Microsoft.EntityFrameworkCore;
using AgileRap_Process_Software_ModelV2.Models;

namespace AgileRap_Process_Software_ModelV2.Data
{
    public class AgileRap_Process_Software_Context : DbContext
    {
        public DbSet<Work> Work { get; set; }
        public DbSet<WorkLog> WorkLog { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Provider> Provider { get; set; }
        public DbSet<ProviderLog> ProviderLog { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-HL7VAK9\SQLEXPRESS;Initial Catalog=AgileRap_Process_Software;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;MultipleActiveResultSets=true");
        }
    }
}
