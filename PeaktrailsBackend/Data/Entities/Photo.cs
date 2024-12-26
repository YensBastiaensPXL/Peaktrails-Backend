using System.ComponentModel.DataAnnotations.Schema;

namespace PeaktrailsBackend.Data.Entities
{
    [Table("TrailPhotos")]
    public class Photo
    {
        public int PhotoId { get; set; }

        public int TrailId { get; set; }

        public byte[] PhotoData { get; set; }  // Store binary data

        public string PhotoDescription { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
