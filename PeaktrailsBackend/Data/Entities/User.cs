using PeaktrailsBackend.Data.Entities;
using System.Text.Json.Serialization;

namespace PeaktrailsApp.Data.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        [JsonIgnore]
        public ICollection<FavoriteTrail> FavoriteTrails { get; set; }
        [JsonIgnore]
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        public string FormattedCreatedDate
        {
            get
            {
                return CreatedDate.ToString("dd-MM-yyyy");
            }
        }
    }

}
