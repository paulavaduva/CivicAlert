using CivicAlert.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Context
{
    public class CivicAlertContext : IdentityDbContext<User>
    {
        public CivicAlertContext(DbContextOptions<CivicAlertContext> options) : base(options) { }

        public DbSet<Category> Categories  { get; set; }
        public DbSet<Issue> Issues { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
