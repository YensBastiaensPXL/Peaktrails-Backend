using Microsoft.AspNetCore.Mvc;
using PeaktrailsBackend.Data;
using PeaktrailsBackend.Data.Entities;
using System.Text;

namespace PeaktrailsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrailsController : ControllerBase
    {
        private readonly TrailsRepository _trailsRepository;
        private readonly PhotosRepository _photosRepository;

        public TrailsController(TrailsRepository trailsRepository, PhotosRepository photosRepository)
        {
            _trailsRepository = trailsRepository ?? throw new ArgumentNullException(nameof(trailsRepository));
            _photosRepository = photosRepository ?? throw new ArgumentNullException(nameof(photosRepository));
        }


        [HttpGet]
        public async Task<IActionResult> GetTrails()
        {
            var trails = await _trailsRepository.GetAllTrailsAsync();

            if (trails == null || !trails.Any())
            {
                return NotFound("No trails found.");
            }

            return Ok(trails);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrail(int id) // Wijziging hier
        {
            var trail = await _trailsRepository.GetTrailByIdAsync(id); // Wijziging hier

            if (trail == null)
            {
                return NotFound();
            }

            return Ok(new { trail, gpxContent = trail.GPXContent }); // Wijziging hier
        }


        [HttpPost("upload-trail")]
        public async Task<IActionResult> UploadGpxFile(
        [FromForm] IFormFile gpxFile,
        [FromForm] int userid,
        [FromForm] string name,
        [FromForm] string distance,
        [FromForm] string ascent,
        [FromForm] string descent,
        [FromForm] string difficulty,
        [FromForm] string description,
        [FromForm] string location,
        [FromForm] IFormFile[] photoFiles)
        {
            if (gpxFile == null || gpxFile.Length == 0)
                return BadRequest("GPX file is required.");

            // Opslaan van het GPX-bestand
            var filePath = Path.Combine(Path.GetTempPath(), gpxFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await gpxFile.CopyToAsync(stream);
            }

            string gpxContent;
            using (var reader = new StreamReader(filePath))
            {
                gpxContent = await reader.ReadToEndAsync();
            }

            // Parse en valideer afstand, stijging en daling
            if (!decimal.TryParse(distance, out var parsedDistance))
                return BadRequest("Invalid distance provided.");
            if (!decimal.TryParse(ascent, out var parsedAscent))
                return BadRequest("Invalid ascent provided.");
            if (!decimal.TryParse(descent, out var parsedDescent))
                return BadRequest("Invalid descent provided.");

            // Maak een nieuwe Trail-entiteit en sla deze op in de database
            var route = new Trail
            {
                Name = name,
                UserId = userid,
                Length = parsedDistance,
                Elevation = parsedAscent,
                TotalAscent = parsedAscent,
                TotalDescent = parsedDescent,
                Difficulty = difficulty,
                Description = description,
                Location = location,
                CreatedDate = DateTime.Now,
                GPXLocation = filePath,
                GPXContent = gpxContent
            };

            // Foto's opslaan en koppelen aan de trail
            if (photoFiles != null && photoFiles.Length > 0)
            {
                foreach (var photoFile in photoFiles)
                {
                    var photoPath = Path.Combine(Path.GetTempPath(), photoFile.FileName);
                    using (var stream = new FileStream(photoPath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(stream);
                    }

                    var photo = new Photo
                    {
                        PhotoData = await System.IO.File.ReadAllBytesAsync(photoPath),
                        PhotoDescription = route.Name,
                        CreatedDate = DateTime.Now,
                        TrailId = route.TrailId // Verbindt de foto met de trail
                    };

                    route.Photos.Add(photo); // Voeg de foto toe aan de trail
                }
            }

            // Sla de trail op in de database
            await _trailsRepository.AddTrailAsync(route);

            return Ok(new { message = "Route saved successfully", route.TrailId });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrail(
     int id,
     [FromForm] IFormFile gpxFile,
     [FromForm] int userid,
     [FromForm] string name,
     [FromForm] string distance,
     [FromForm] string ascent,
     [FromForm] string descent,
     [FromForm] string difficulty,
     [FromForm] string description,
     [FromForm] string location,
     [FromForm] IFormFile[] photoFiles)
        {
            try
            {
                // Haal de bestaande trail op
                var existingTrail = await _trailsRepository.GetTrailByIdAsync(id);

                if (existingTrail == null)
                {
                    return NotFound($"Trail with id {id} not found.");
                }

                // Parse en valideer afstand, stijging en daling
                if (!decimal.TryParse(distance, out var parsedDistance))
                    return BadRequest("Invalid distance provided.");
                if (!decimal.TryParse(ascent, out var parsedAscent))
                    return BadRequest("Invalid ascent provided.");
                if (!decimal.TryParse(descent, out var parsedDescent))
                    return BadRequest("Invalid descent provided.");

                // Werk de trailgegevens bij
                existingTrail.UserId = userid;
                existingTrail.Name = name;
                existingTrail.Length = parsedDistance;
                existingTrail.TotalAscent = parsedAscent;
                existingTrail.TotalDescent = parsedDescent;
                existingTrail.Difficulty = difficulty;
                existingTrail.Description = description;
                existingTrail.Location = location;

                // GPX-bestand verwerken als een nieuw bestand wordt geüpload
                if (gpxFile != null && gpxFile.Length > 0)
                {
                    var filePath = Path.Combine(Path.GetTempPath(), gpxFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await gpxFile.CopyToAsync(stream);
                    }

                    using (var reader = new StreamReader(filePath))
                    {
                        existingTrail.GPXContent = await reader.ReadToEndAsync();
                    }

                    existingTrail.GPXLocation = filePath;
                }

                // Verwijder oude foto's en upload nieuwe
                if (photoFiles != null && photoFiles.Length > 0)
                {
                    // Verwijder bestaande foto's
                    await _photosRepository.DeletePhotosByTrailId(existingTrail.TrailId);

                    // Voeg nieuwe foto's toe
                    foreach (var file in photoFiles)
                    {
                        byte[] fileData;
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            fileData = memoryStream.ToArray();
                        }

                        var newPhoto = new Photo
                        {
                            TrailId = existingTrail.TrailId,
                            PhotoData = fileData,
                            PhotoDescription = name,
                            CreatedDate = DateTime.Now
                        };

                        await _photosRepository.AddPhoto(newPhoto);
                    }
                }

                // Update trail in de database
                await _trailsRepository.UpdateTrailAsync(existingTrail);
                return Ok(new { message = "Trail updated successfully", trailId = existingTrail.TrailId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the trail: {ex.Message}");
            }
        }










        [HttpGet("{trailId}/download-gpx")]
        public async Task<IActionResult> DownloadGpxFile(int trailId)
        {
            var trail = await _trailsRepository.GetTrailByIdAsync(trailId);
            if (trail == null)
            {
                return NotFound("Trail not found.");
            }

            if (string.IsNullOrEmpty(trail.GPXContent))
            {
                return NotFound("No GPX data available for this trail.");
            }

            // Convert de GPX-content (die als string is opgeslagen) naar een byte array
            var gpxData = Encoding.UTF8.GetBytes(trail.GPXContent);

            // Stel een geschikte bestandsnaam in, inclusief de .gpx extensie
            var fileName = $"{trail.Name.Replace(" ", "_")}.gpx";

            // Return een bestand met de GPX data
            return File(gpxData, "application/gpx+xml", fileName);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrail(int id)
        {
            try
            {
                var trail = await _trailsRepository.GetTrailByIdAsync(id);
                if (trail == null)
                {
                    return NotFound($"Trail with id {id} not found.");
                }


                await _trailsRepository.DeleteTrailAsync(id);

                return Ok(new { message = "Trail successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting trail: {ex.Message}");
            }
        }

        // Haal alle reviews voor een specifieke trail op
        [HttpGet("{trailId}/reviews")]
        public async Task<IActionResult> GetReviewsByTrail(int trailId)
        {
            try
            {
                var reviews = await _trailsRepository.GetReviewsByTrailIdAsync(trailId);
                if (reviews == null || !reviews.Any())
                {
                    return NotFound("Geen reviews gevonden voor deze trail.");
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching reviews: {ex.Message}");
            }
        }

        // Voeg een review toe voor een specifieke trail
        [HttpPost("{trailId}/reviews")]
        public async Task<IActionResult> AddReview(int trailId, [FromBody] ReviewDto reviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var review = new Review
                {
                    TrailId = trailId,
                    UserId = reviewDto.UserId, // Je kan de userId hier uit de frontend halen, bijvoorbeeld uit de sessie
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    CreatedDate = DateTime.Now
                };

                await _trailsRepository.AddReviewAsync(review);

                return Ok(new { message = "Review added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding review: {ex.Message}");
            }
        }

    }
}
