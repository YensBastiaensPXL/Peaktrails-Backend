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
        public DbSet<FavoriteTrail> FavoriteTrails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }

    }
}
