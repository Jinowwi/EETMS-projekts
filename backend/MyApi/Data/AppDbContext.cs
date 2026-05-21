using Microsoft.EntityFrameworkCore; 
using MyApi.Models; 

namespace MyApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Administration> Administrators { get; set; } 
        public DbSet<CompanyReason> CompanyReasons { get; set; }
        public DbSet<ShiftRequest> ShiftRequests { get; set; }
        public DbSet<Ratings> Ratings { get; set; }
        public DbSet<PlannedShifts> PlannedShifts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Administration>()
                .HasKey(a => a.RemID); 

            modelBuilder.Entity<CompanyReason>()
                .HasKey(cr => cr.CompanyReasonID);
    
            modelBuilder.Entity<CompanyReason>()
                .HasOne(cr => cr.Company)
                .WithMany(c => c.CompanyReasons)
                .HasForeignKey(cr => cr.CompaniesCompanyID);
            
            modelBuilder.Entity<CompanyReason>()
                .HasOne(cr => cr.Reason)
                .WithMany(r => r.CompanyReasons)
                .HasForeignKey(cr => cr.ReasonsReasonID);
            
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.CompanyReason)
                .WithMany()
                .HasForeignKey(s => s.CompanyReasonID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Shop)
                .WithMany(sh => sh.Shifts)
                .HasForeignKey(s => s.ShopID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShiftRequest>(entity =>
            {
                entity.HasKey(e => e.ShiftRequestID);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.HasOne(e => e.Reason)
                    .WithMany()
                    .HasForeignKey(e => e.ReasonID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Rem)
                    .WithMany()
                    .HasForeignKey(e => e.RemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Shop)
                    .WithMany()
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ratings>(entity =>
            {
                entity.HasKey(e => e.RatingID);

                entity.Property(e => e.Stars)
                    .IsRequired();

                entity.HasOne(e => e.ShiftRequest)
                    .WithMany()
                    .HasForeignKey(e => e.ShiftRequestID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlannedShifts>(entity =>
            {
                entity.HasKey(e => e.PlannedShiftsID);

                entity.Property(e => e.approx_start_time)
                    .IsRequired();

                entity.Property(e => e.approx_date)
                    .IsRequired();

                entity.Property(e => e.approx_duration)
                    .IsRequired();

                entity.HasOne(e => e.ShiftRequest)
                    .WithOne(p => p.PlannedShift)
                    .HasForeignKey<PlannedShifts>(e => e.ShiftRequestID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}