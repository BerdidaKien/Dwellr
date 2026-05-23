using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dwellr.Models
{
    public class DwellrDbContext : IdentityDbContext<AppUser>
    {
        public DwellrDbContext(DbContextOptions<DwellrDbContext> options)
            : base(options) { }

        public DbSet<Property> Properties => Set<Property>();
        public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<SavedListing> SavedListings => Set<SavedListing>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<RentalApplication> RentalApplications => Set<RentalApplication>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedListing>()
                .HasOne(s => s.Property)
                .WithMany()
                .HasForeignKey(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedListing>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.Sender)
                .WithMany(u => u.SentTransactions)
                .HasForeignKey(t => t.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.Receiver)
                .WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.Property)
                .WithMany()
                .HasForeignKey(t => t.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RentalApplication>()
                .HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RentalApplication>()
                .HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RentalApplication>()
                .HasOne(r => r.Transaction)
                .WithMany()
                .HasForeignKey(r => r.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure precision for the user's wallet balance
            builder.Entity<AppUser>()
                .Property(u => u.WalletBalance)
                .HasColumnType("decimal(18,2)");
        }
    }
}