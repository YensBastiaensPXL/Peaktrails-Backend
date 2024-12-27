using Microsoft.AspNetCore.Mvc;
using PeaktrailsBackend.Data;

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


        [HttpPost("upload-gpx")]
        public async Task<IActionResult> UploadGpxFile(
        IFormFile gpxFile,
        [FromForm] string distance,
        [FromForm] string ascent,
        [FromForm] string descent,
        [FromForm] string name,
        [FromForm] string difficulty,
        [FromForm] string description,
        [FromForm] string location)
        {
            if (gpxFile == null || gpxFile.Length == 0)
                return BadRequest("GPX file is required.");

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

            // Parse and validate distance, ascent, and descent
            if (!decimal.TryParse(distance, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedDistance))
                return BadRequest("Invalid distance provided.");
            if (!decimal.TryParse(ascent, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedAscent))
                return BadRequest("Invalid ascent provided.");
            if (!decimal.TryParse(descent, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedDescent))
                return BadRequest("Invalid descent provided.");

            // Create a new Trail entity and save it to the database
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

            // Save the new trail in the database
            await _trailsRepository.AddTrailAsync(route);

            // Return a success message and the TrailId in the response
            return Ok(new { message = "Route saved successfully", route.TrailId });
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

    }
}
