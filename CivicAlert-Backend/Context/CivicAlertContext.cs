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
        public DbSet<Department> Departments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Issue>()
                .HasOne(i => i.Reporter)
                .WithMany(u => u.Issues)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Issue>()
                .HasOne(i => i.Dispatcher)
                .WithMany() 
                .HasForeignKey(i => i.DispatcherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Issue>()
                .HasOne(i => i.AssignedToUser)
                .WithMany()
                .HasForeignKey(i => i.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Categories)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
