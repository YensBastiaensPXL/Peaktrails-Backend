using System.ComponentModel.DataAnnotations;

namespace PeaktrailsBackend.Data.Entities
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Gebruikersinvoer
        [Required]
        public string Password { get; set; } // Gebruikersinvoer
    }

}
