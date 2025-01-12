using System.ComponentModel.DataAnnotations.Schema;

namespace PeaktrailsBackend.Data.Entities
{
    [Table("TrailPhotos")]
    public class Photo
    {
        public int PhotoId { get; set; }

        public int TrailId { get; set; }

        public byte[] PhotoData { get; set; }

        public string PhotoDescription { get; set; }

        public DateTime CreatedDate { get; set; }


    }
}
