using Microsoft.AspNetCore.Mvc;
using PeaktrailsBackend.Data;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrailsController : ControllerBase // Wijziging hier
    {
        private readonly TrailsRepository _trailsRepository; // Wijziging hier

        public TrailsController(PeaktrailsAppContext context)
        {
            _trailsRepository = new TrailsRepository(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrails()
        {
            var trails = await _trailsRepository.GetAllTrailsAsync();

            // Controleer of trails null is of een lege lijst
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
    [FromForm] string name,
    [FromForm] string distance,
    [FromForm] string ascent,
    [FromForm] string descent,
    [FromForm] string difficulty,
    [FromForm] string description,
    [FromForm] string location,
    [FromForm] IFormFile[] photoFiles) // Gebruik een array van foto-bestanden
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
    [FromForm] IFormFile gpxFile,  // Nieuw bestand toevoegen
    [FromForm] string name,
    [FromForm] string distance,
    [FromForm] string ascent,
    [FromForm] string descent,
    [FromForm] string difficulty,
    [FromForm] string description,
    [FromForm] string location)
        {
            var trail = await _trailsRepository.GetTrailByIdAsync(id);

            if (trail == null)
            {
                return NotFound($"Trail with id {id} not found.");
            }

            // Parse and validate distance, ascent, and descent
            if (!decimal.TryParse(distance, out var parsedDistance))
                return BadRequest("Invalid distance provided.");
            if (!decimal.TryParse(ascent, out var parsedAscent))
                return BadRequest("Invalid ascent provided.");
            if (!decimal.TryParse(descent, out var parsedDescent))
                return BadRequest("Invalid descent provided.");

            // Update trail properties
            trail.Name = name;
            trail.Length = parsedDistance;
            trail.Elevation = parsedAscent;
            trail.TotalAscent = parsedAscent;
            trail.TotalDescent = parsedDescent;
            trail.Difficulty = difficulty;
            trail.Description = description;
            trail.Location = location;

            // If a new GPX file is provided, process it
            if (gpxFile != null && gpxFile.Length > 0)
            {
                // Save the GPX file to a temporary location
                var filePath = Path.Combine(Path.GetTempPath(), gpxFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await gpxFile.CopyToAsync(stream);
                }

                // Read the GPX content from the saved file
                string gpxContent;
                using (var reader = new StreamReader(filePath))
                {
                    gpxContent = await reader.ReadToEndAsync();
                }

                // Update GPX fields in the trail
                trail.GPXLocation = filePath;
                trail.GPXContent = gpxContent;
            }

            // Update the trail in the database
            await _trailsRepository.UpdateTrailAsync(trail);

            return Ok(new { message = "Trail updated successfully", trail.TrailId });
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
