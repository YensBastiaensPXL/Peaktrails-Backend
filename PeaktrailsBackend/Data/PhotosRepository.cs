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
    }
}
