using System.ComponentModel.DataAnnotations;

namespace PeaktrailsApp.Data.Models
{
    public class UserDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
