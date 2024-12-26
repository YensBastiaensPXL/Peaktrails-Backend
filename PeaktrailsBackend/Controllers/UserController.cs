using Microsoft.AspNetCore.Mvc;
using PeaktrailsApp.Data;
using PeaktrailsApp.Data.Entities;
using PeaktrailsApp.Data.Models;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsApp.Controllers
{
    // API-controller voor User-gerelateerde acties
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersRepository _repository;

        public UsersController(UsersRepository repository)
        {
            _repository = repository; // Injectie van de repository
        }



        // Haalt alle gebruikers op
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repository.GetUsersAsync();
            return Ok(users);
        }

        // Haalt een specifieke gebruiker op via ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repository.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserDto userDto)
        {
            // Controleer of het model geldig is
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retourneer validatiefouten
            }

            // Controleer of de gebruiker al bestaat (bijvoorbeeld via e-mail)
            var existingUser = await _repository.GetUsersAsync();
            if (existingUser.Any(u => u.Email == userDto.Email))
            {
                return Conflict("Een gebruiker met dit e-mailadres bestaat al.");
            }

            // Maak een nieuwe User-entiteit aan op basis van de DTO
            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash)
            };

            // Voeg de gebruiker toe aan de database
            await _repository.AddUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }
        // Inloggen gebruiker

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Controleer of de gebruiker bestaat
            var user = await _repository.GetUsersAsync();
            var existingUser = user.FirstOrDefault(u => u.Email == loginDto.Email);

            if (existingUser == null)
            {
                return Unauthorized("Gebruiker niet gevonden.");
            }

            // Controleer het wachtwoord
            var isPasswordValid = PasswordHasher.VerifyPassword(loginDto.Password, existingUser.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized("Ongeldig wachtwoord.");
            }

            return Ok(new { UserId = existingUser.UserId, UserName = existingUser.UserName });
        }
        // Favorieten ophalen user
        [HttpGet("{userId}/favorites")]
        public async Task<IActionResult> GetFavoriteTrails(int userId)
        {
            try
            {
                // Haal de favoriete trails van de gebruiker op
                var favoriteTrails = await _repository.GetFavoriteTrailsByUserIdAsync(userId);

                // Controleer of er geen favoriete trails zijn
                if (!favoriteTrails.Any())
                    return NotFound("Geen favoriete trails gevonden voor deze gebruiker.");

                // Verwerk de foto’s en zet de foto’s om naar Base64 voor verzending
                var trailWithPhotos = favoriteTrails.Select(trail => new
                {
                    Trail = new
                    {
                        trail.TrailId,
                        trail.Name,
                        trail.Length,
                        trail.CreatedDate,
                        trail.Difficulty,
                        trail.Description,
                        trail.Elevation,
                        trail.TotalAscent,
                        trail.TotalDescent,
                        trail.Location,
                        trail.GPXContent,
                        trail.GPXLocation,
                        Photos = trail.Photos.Select(p => new
                        {
                            p.PhotoId,
                            p.PhotoDescription,
                            PhotoData = Convert.ToBase64String(p.PhotoData), // Zet de foto om naar base64
                            p.CreatedDate
                        }).ToList() // Zet de foto's om naar een lijst
                    }
                }).ToList();

                return Ok(trailWithPhotos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Er is een fout opgetreden bij het ophalen van de favoriete trails: {ex.Message}");
            }
        }




        // Favoriet toevoegen gebruiker
        [HttpPost("{userId}/addfavorites/{trailId}")]
        public async Task<IActionResult> AddFavoriteTrail(int userId, int trailId)
        {
            try
            {
                // Add the favorite trail to the database using the repository
                await _repository.AddFavoriteTrailAsync(userId, trailId);

                return Ok(); // Return success
            }
            catch (Exception ex)
            {
                // Return an error if something goes wrong
                return StatusCode(500, $"Error adding favorite: {ex.Message}");
            }
        }
    }
}
