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

        [Required]
        [Compare("PasswordHash", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }  // Bevestig wachtwoord
    }
}
