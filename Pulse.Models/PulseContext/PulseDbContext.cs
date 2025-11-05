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
using Pulse.Models.Industries;
using Pulse.Models.Rollers;
using Pulse.Models.Compounds;
using Pulse.Models.Users;
using Pulse.Models.Production;

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
        public DbSet<SalesRepresentative> RepresentativeMaster { get; set; }
        public DbSet<ClientSales> ClientSales {  get; set; }
        public DbSet<Period> PeriodMaster { get; set; }
        public DbSet<IndustryProcess> IndustryProcessMaster { get; set; }
        public DbSet<RollerType> RollerTypeMaster { get; set; }
        public DbSet<ShellType> ShellTypeMaster { get; set; }
        public DbSet<Polymer> PolymerMaster { get; set; }
        public DbSet<Colour> ColourMaster { get; set; }
        public DbSet<HardnessType> HardnessTypeMaster { get; set; }
        public DbSet<CompoundRange> CompoundRangeMaster { get; set; }
        public DbSet<CompoundRangeProperty> CompoundRangePropertyMaster { get; set; }
        public DbSet<Compound> CompoundMaster { get; set; }
        public DbSet<IndustryRollerEnvironment> IndustryRollerEnvironmentMaster { get; set; }
        public DbSet<IndustryRecommendedCover> IndustryRecommendedCoverMaster { get; set; }
        public DbSet<ClientContact> ClientContactMaster { get; set; }
        public DbSet<ClientRollerSpecification> ClientRollerSpecificationMaster { get; set; }
        public DbSet<ClientRoller> ClientRollerMaster { get; set; }
        public DbSet<UserFavouriteQry> UserFavouriteQueries {  get; set; }
        public DbSet<WorkType> WorkTypeMaster { get; set; }
        public DbSet<WorksOrder> WorksOrder { get; set; }

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
            modelBuilder.ApplyConfiguration(new RepresentativeMasterMap());
            modelBuilder.ApplyConfiguration(new ClientSalesMap());
            modelBuilder.ApplyConfiguration(new PeriodMasterMap());
            modelBuilder.ApplyConfiguration(new IndustryProcessMasterMap());
            modelBuilder.ApplyConfiguration(new RollerTypeMasterMap());
            modelBuilder.ApplyConfiguration(new ShellTypeMasterMap());
            modelBuilder.ApplyConfiguration(new PolymerMasterMap());
            modelBuilder.ApplyConfiguration(new ColourMasterMap());
            modelBuilder.ApplyConfiguration(new HardnessTypeMasterMap());
            modelBuilder.ApplyConfiguration(new CompoundRangeMasterMap());
            modelBuilder.ApplyConfiguration(new CompoundRangePropertyMasterMap());
            modelBuilder.ApplyConfiguration(new CompoundMasterMap());
            modelBuilder.ApplyConfiguration(new IndustryRollerEnvironmentMasterMap());
            modelBuilder.ApplyConfiguration(new IndustryRecommendedCoverMasterMap());
            modelBuilder.ApplyConfiguration(new ClientContactMasterMap());
            modelBuilder.ApplyConfiguration(new ClientRollerSpecificationMasterMap());
            modelBuilder.ApplyConfiguration(new ClientRollerMasterMap());
            modelBuilder.ApplyConfiguration(new UserFavouriteQueryMap());
            modelBuilder.ApplyConfiguration(new WorkTypeMasterMap());
            modelBuilder.ApplyConfiguration(new WorksOrderMap());


            base.OnModelCreating(modelBuilder);
        }
    }
}
