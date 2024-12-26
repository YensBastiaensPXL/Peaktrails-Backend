using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeaktrailsBackend.Data;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly PeaktrailsAppContext _context;

        public PhotosController(PeaktrailsAppContext context)
        {
            _context = context;
        }

        [HttpGet("trail/{trailId}")]
        public async Task<IActionResult> GetPhotosByTrailId(int trailId)
        {
            var photos = await _context.TrailPhotos
                .Where(p => p.TrailId == trailId)
                .ToListAsync();

            if (photos == null || !photos.Any())
            {
                return NotFound("No photos found for the specified trail.");
            }

            // Return the photos as a list of objects with their binary data
            var photoResults = photos.Select(photo => new
            {
                photo.PhotoId,
                photo.PhotoDescription,
                photo.CreatedDate,
                // Here you might want to return the base64 string for binary data
                PhotoData = Convert.ToBase64String(photo.PhotoData)  // Convert binary to Base64 
            });

            return Ok(photoResults);
        }


        [HttpPost("{trailId}/upload")]
        public async Task<IActionResult> UploadPhotos(int trailId, [FromForm] IFormFile[] files, [FromForm] string description)
        {
            try
            {
                if (files == null || files.Length == 0)
                {
                    return BadRequest("No files were uploaded.");
                }

                var uploadedPhotos = new List<Photo>();

                foreach (var file in files)
                {
                    // Convert file to byte array (binary data)
                    byte[] fileData;
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }

                    var photo = new Photo
                    {
                        TrailId = trailId,
                        PhotoData = fileData,
                        PhotoDescription = description,
                        CreatedDate = DateTime.Now
                    };

                    _context.TrailPhotos.Add(photo);
                    uploadedPhotos.Add(photo);
                }

                await _context.SaveChangesAsync();

                return Ok(uploadedPhotos.Select(p => new
                {
                    p.PhotoId,
                    p.PhotoDescription,
                    p.CreatedDate
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error occurred: {ex.Message}");
            }
        }
    }
}
