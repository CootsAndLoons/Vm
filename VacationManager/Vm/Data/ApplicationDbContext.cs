using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vm.Models;

namespace Vm.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for existing models
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<VacationRequest> VacationRequests { get; set; }

        // Add DbSet for LeaveHistory
        public DbSet<LeaveHistory> LeaveHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Developers)
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Team>()
                .HasOne(t => t.TeamLead)
                .WithMany()
                .HasForeignKey(t => t.TeamLeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Team>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Teams)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VacationRequest>()
                .HasOne(v => v.Requester)
                .WithMany(u => u.VacationRequests)
                .HasForeignKey(v => v.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LeaveHistory relationships
            builder.Entity<LeaveHistory>()
                .HasOne(lh => lh.Requester)
                .WithMany(u => u.LeaveHistories)
                .HasForeignKey(lh => lh.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaveHistory>()
                .HasOne(lh => lh.ApprovalUser)
                .WithMany()
                .HasForeignKey(lh => lh.ApprovalUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
