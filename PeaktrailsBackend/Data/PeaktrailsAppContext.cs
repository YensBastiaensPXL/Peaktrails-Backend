using Microsoft.EntityFrameworkCore;
using PeaktrailsApp.Data.Entities;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsBackend.Data
{
    public class PeaktrailsAppContext : DbContext
    {
        public PeaktrailsAppContext(DbContextOptions<PeaktrailsAppContext> options) : base(options)
        {
        }

        public DbSet<Trail> Trails { get; set; }
        public DbSet<Photo> TrailPhotos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FavoriteTrail> FavoriteTrails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Trail>()
                .HasOne<User>() // Stel dat een Trail één User heeft
                .WithMany() // En een User kan veel Trails hebben
                .HasForeignKey(p => p.UserId) // Foreign Key op UserId
                .OnDelete(DeleteBehavior.Cascade); // Of een andere DeleteBehavior afhankelijk van je vereisten

            // Relatie tussen Trail en Photo (one-to-many)
            modelBuilder.Entity<Photo>()
                .HasOne<Trail>()
                .WithMany(t => t.Photos) // Eén Trail heeft veel Photos
                .HasForeignKey(p => p.TrailId)
                .IsRequired();

            //// Relatie tussen FavoriteTrail en Trail (one-to-many)
            //modelBuilder.Entity<FavoriteTrail>()
            //    .HasOne(ft => ft.Trail)
            //    .WithMany(t => t.FavoriteTrails) // Eén Trail kan veel FavoriteTrails hebben
            //    .HasForeignKey(ft => ft.TrailId)
            //    .IsRequired();

            // Relatie tussen FavoriteTrail en User (many-to-one)
            modelBuilder.Entity<FavoriteTrail>()
                .HasOne(ft => ft.User)
                .WithMany(u => u.FavoriteTrails) // Eén User kan veel FavoriteTrails hebben
                .HasForeignKey(ft => ft.UserId)
                .IsRequired();

            // Relatie tussen Review en User (many-to-one)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)  // Review heeft een User
                .WithMany(u => u.Reviews) // Eén User kan veel Reviews hebben
                .HasForeignKey(r => r.UserId)
                .IsRequired();

            // Relatie tussen Review en Trail (many-to-one)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Trail)  // Review heeft een Trail
                .WithMany(t => t.Reviews) // Eén Trail kan veel Reviews hebben
                .HasForeignKey(r => r.TrailId)
                .IsRequired();
        }

    }
}
