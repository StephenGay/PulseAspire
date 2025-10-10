using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.PulseContext.Maps;
using Pulse.Models.Customers;
using Pulse.Models.Organizational;
using Pulse.Models.Geographic;
using Pulse.Models.Misc;

namespace Pulse.Models.PulseContext
{
    public sealed class PulseDbContext(DbContextOptions<PulseDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> ClientMaster { get; set; }
        public DbSet<Company> CompanyMaster { get; set; }
        public DbSet<Branch> BranchMaster { get; set; }
        public DbSet<Division> DivisionMaster { get; set; }
        public DbSet<Continent> ContinentMaster { get; set; }
        public DbSet<Country> CountryMaster { get; set; }
        public DbSet<Province> ProvinceMaster { get; set; }
        public DbSet<Region> RegionMaster { get; set; }
        public DbSet<Industry> IndustryMaster { get; set; }
        public DbSet<User> UserMaster { get; set; }
        public DbSet<AiQuery> AiSavedQueries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClientMasterMap());
            modelBuilder.ApplyConfiguration(new CompanyMasterMap());
            modelBuilder.ApplyConfiguration(new BranchMasterMap());
            modelBuilder.ApplyConfiguration(new DivisionMasterMap());
            modelBuilder.ApplyConfiguration(new ContinentMasterMap());
            modelBuilder.ApplyConfiguration(new CountryMasterMap());
            modelBuilder.ApplyConfiguration(new ProvinceMasterMap());
            modelBuilder.ApplyConfiguration(new RegionMasterMap());
            modelBuilder.ApplyConfiguration(new IndustryMasterMap());
            modelBuilder.ApplyConfiguration(new UserMasterMap());
            modelBuilder.ApplyConfiguration(new AiSavedQueriesMap());

            base.OnModelCreating(modelBuilder);
        }
    }
}
