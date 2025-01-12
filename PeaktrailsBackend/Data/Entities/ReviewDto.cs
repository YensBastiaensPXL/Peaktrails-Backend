namespace PeaktrailsBackend.Data.Entities
{
    public class ReviewDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

}
