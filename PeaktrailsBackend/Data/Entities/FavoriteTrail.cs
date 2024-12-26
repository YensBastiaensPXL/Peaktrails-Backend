namespace PeaktrailsApp.Data.Entities
{
    public class FavoriteTrail
    {
        public int FavoriteTrailId { get; set; } // Primaire sleutel

        // Relatie met User
        public int UserId { get; set; }
        public User User { get; set; }

        // Relatie met Trail
        public int TrailId { get; set; }
        public Trail Trail { get; set; }
    }
}
