using Microsoft.EntityFrameworkCore;
using PeaktrailsBackend.Data.Entities;

namespace PeaktrailsBackend.Data
{
    public class TrailsRepository // Wijziging hier
    {
        private readonly PeaktrailsAppContext _context;

        public TrailsRepository(PeaktrailsAppContext context)
        {
            _context = context;
        }

        public async Task<List<Trail>> GetAllTrailsAsync() // Wijziging hier
        {
            return await _context.Trails.ToListAsync(); // Wijziging hier
        }

        public async Task<Trail> GetTrailByIdAsync(int id) // Wijziging hier
        {
            return await _context.Trails.FindAsync(id); // Wijziging hier
        }

        public async Task AddTrailAsync(Trail trail) // Wijziging hier
        {
            await _context.Trails.AddAsync(trail); // Wijziging hier
            await _context.SaveChangesAsync();
        }

        public async Task RemovePhotosByTrailIdAsync(int trailId)
        {
            var photos = await _context.TrailPhotos
                .Where(p => p.TrailId == trailId)
                .ToListAsync();

            if (photos.Any())
            {
                _context.TrailPhotos.RemoveRange(photos);
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateTrailAsync(Trail trail)
        {
            // Haal de bestaande trail op
            var existingTrail = await _context.Trails
                .Include(t => t.Photos)
                .FirstOrDefaultAsync(t => t.TrailId == trail.TrailId);

            if (existingTrail != null)
            {


                // Update trail properties
                existingTrail.Name = trail.Name;
                existingTrail.Length = trail.Length;
                existingTrail.TotalAscent = trail.TotalAscent;
                existingTrail.TotalDescent = trail.TotalDescent;
                existingTrail.Difficulty = trail.Difficulty;
                existingTrail.Description = trail.Description;
                existingTrail.Location = trail.Location;
                existingTrail.GPXContent = trail.GPXContent;
                existingTrail.GPXLocation = trail.GPXLocation;

                // Voeg de nieuwe foto's toe
                existingTrail.Photos = new List<Photo>(trail.Photos);

                // Save changes
                await _context.SaveChangesAsync();
            }
        }









        public async Task DeleteTrailAsync(int id)
        {
            var trail = await _context.Trails
                                      .Include(t => t.Photos)
                                      .Include(t => t.FavoriteTrails)
                                      .FirstOrDefaultAsync(t => t.TrailId == id);

            if (trail != null)
            {
                if (trail.Photos != null)
                {
                    _context.TrailPhotos.RemoveRange(trail.Photos);
                }

                if (trail.FavoriteTrails != null)
                {
                    _context.FavoriteTrails.RemoveRange(trail.FavoriteTrails);
                }

                _context.Trails.Remove(trail);

                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<ReviewDto>> GetReviewsByTrailIdAsync(int trailId)
        {
            return await _context.Reviews
                .Where(r => r.TrailId == trailId)
                .Join(
                    _context.Users,
                    review => review.UserId,
                    user => user.UserId,
                    (review, user) => new ReviewDto
                    {
                        UserId = review.UserId,
                        UserName = user.UserName,
                        Rating = review.Rating,
                        Comment = review.Comment
                    }
                )
                .ToListAsync();
        }


        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }




    }
}
