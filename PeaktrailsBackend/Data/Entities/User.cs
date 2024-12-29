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

        //JsonIgnore wordt gebruikt zodat je deze niet mee stuurt in de json response
        [JsonIgnore]
        public ICollection<FavoriteTrail> FavoriteTrails { get; set; }
        [JsonIgnore]
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        // Een extra eigenschap die de datum formatteert naar 'dd-MM-yyyy'
        public string FormattedCreatedDate
        {
            get
            {
                return CreatedDate.ToString("dd-MM-yyyy");
            }
        }
    }

}
