﻿using Microsoft.AspNetCore.Mvc;
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
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repository.GetUsersAsync();
            return Ok(users);
        }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                CreatedDate = DateTime.Now,
                PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash)
            };

            // Voeg de gebruiker toe aan de database
            await _repository.AddUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            // Haal de gebruiker op uit de database
            var user = await _repository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User with id {id} not found.");
            }

            // Controleer of het model geldig is
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retourneer validatiefouten
            }

            // Check of het wachtwoord en confirm password overeenkomen
            if (userDto.PasswordHash != userDto.ConfirmPassword)
            {
                return BadRequest("Password and confirmation password do not match.");
            }

            // Werk de andere gegevens bij
            user.UserName = userDto.UserName;
            user.Email = userDto.Email;

            // Wachtwoord bijwerken (wachtwoord moet worden gehashed)
            user.PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash);

            // Sla de wijzigingen op in de database
            await _repository.UpdateUserAsync(user);

            return Ok(new { message = "User updated successfully", user.UserId });
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


        [HttpGet("{userId}/trails")]
        public async Task<IActionResult> GetTrailsByUserId(int userId)
        {
            var userTrails = await _repository.GetTrailsByUserIdAsync(userId);
            if (userTrails == null || userTrails.Count == 0)
            {
                return NotFound("Geen trails gevonden voor deze gebruiker.");
            }
            return Ok(userTrails);
        }

        [HttpDelete("{userId}/trails/{trailId}")]
        public async Task<IActionResult> DeleteUserTrail(int userId, int trailId)
        {
            try
            {
                var result = await _repository.DeleteUserTrailAsync(userId, trailId);
                if (!result)
                {
                    return NotFound("Trail not found or not associated with the user.");
                }
                return Ok("Trail deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting trail: {ex.Message}");
            }
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
                if (favoriteTrails == null || !favoriteTrails.Any())
                {
                    // Geen favoriete trails gevonden, retourneer een lege lijst
                    return Ok(new List<Trail>());
                }
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


        [HttpPost("{userId}/addfavorites/{trailId}")]
        public async Task<IActionResult> AddFavoriteTrail(int userId, int trailId)
        {
            try
            {
                // Voeg de favoriete trail toe en controleer of het al een favoriet is
                var result = await _repository.AddFavoriteTrailAsync(userId, trailId);

                if (result)
                {
                    return Ok("Favorite trail added successfully.");
                }
                else
                {
                    return Conflict("This trail is already in your favorites.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding favorite: {ex.Message}");
            }
        }



        // Verwijder een favoriete trail van een gebruiker
        [HttpDelete("{userId}/removefavorites/{trailId}")]
        public async Task<IActionResult> RemoveFavoriteTrail(int userId, int trailId)
        {
            try
            {
                // Verwijder de favoriete trail uit de database
                var result = await _repository.RemoveFavoriteTrailAsync(userId, trailId);

                if (result)
                {
                    return Ok("Favorite trail removed successfully.");
                }
                else
                {
                    return NotFound("Favorite trail not found.");
                }
            }
            catch (Exception ex)
            {
                // Foutafhandeling
                return StatusCode(500, $"Error removing favorite: {ex.Message}");
            }
        }
    }
}
