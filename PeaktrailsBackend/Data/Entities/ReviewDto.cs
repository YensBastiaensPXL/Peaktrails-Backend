namespace PeaktrailsBackend.Data.Entities
{
    public class ReviewDto
    {
        public int UserId { get; set; }  // De id van de gebruiker die de review plaatst
        public int Rating { get; set; }
        public string Comment { get; set; }  // De inhoud van de review
    }

}
