using Microsoft.EntityFrameworkCore;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsBackend.Data
{
    public class PhotosRepository
    {
        private readonly PeaktrailsAppContext _context;

        public PhotosRepository(PeaktrailsAppContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Photo>> GetPhotosByTrailId(int trailId)
        {
            return await _context.TrailPhotos
                .Where(p => p.TrailId == trailId)
                .ToListAsync();
        }

        public async Task<Photo> AddPhoto(Photo photo)
        {
            _context.TrailPhotos.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        public async Task DeletePhoto(int photoId)
        {
            var photo = await _context.TrailPhotos.FindAsync(photoId);
            if (photo != null)
            {
                _context.TrailPhotos.Remove(photo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePhotosByTrailId(int trailId)
        {
            var photos = await _context.TrailPhotos.Where(p => p.TrailId == trailId).ToListAsync();

            if (photos == null || !photos.Any())
            {
                Console.WriteLine($"No photos found for trailId {trailId}");
                return;
            }

            _context.TrailPhotos.RemoveRange(photos);
            await _context.SaveChangesAsync();
        }


    }
}
