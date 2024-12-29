using PeaktrailsApp.Data.Entities;

namespace PeaktrailsBackend.Data.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }  // Verwijzing naar de gebruiker
        public int TrailId { get; set; }  // Verwijzing naar de trail
        public int Rating { get; set; }  // De beoordeling (bijvoorbeeld 1-5)
        public string Comment { get; set; }  // De commentaar
        public DateTime CreatedDate { get; set; }  // De datum waarop de review is gemaakt

        public User User { get; set; }  // Navigatie-eigenschap naar de gebruiker
        public Trail Trail { get; set; }  // Navigatie-eigenschap naar de trail
    }

}
