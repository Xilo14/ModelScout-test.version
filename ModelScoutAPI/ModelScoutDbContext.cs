

using Microsoft.EntityFrameworkCore;
using ModelScoutAPI.Models;

namespace ModelScoutAPI {
    public class ModelScoutDbContext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<VkAcc> VkAccs { get; set; }
        public DbSet<VkClient> VkClients { get; set; }


        public ModelScoutDbContext(DbContextOptions<ModelScoutDbContext> options) 
            :base(options)
        {
            //Database.EnsureCreated();
        }
        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            modelBuilder
                .Entity<VkClient> ()
                .Property (e => e.ClientStatus)
                .HasConversion<int> ();
        }
    }
}