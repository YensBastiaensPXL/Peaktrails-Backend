using Microsoft.EntityFrameworkCore;

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

        public async Task UpdateTrailAsync(Trail trail) // Wijziging hier
        {
            _context.Entry(trail).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTrailAsync(int id)
        {
            var trail = await _context.Trails
                                      .Include(t => t.Photos)  // Includeer foto's
                                      .Include(t => t.FavoriteTrails)  // Includeer favoriete trails
                                      .FirstOrDefaultAsync(t => t.TrailId == id);

            if (trail != null)
            {
                // Verwijder alle foto's die aan de trail zijn gekoppeld
                if (trail.Photos != null)
                {
                    _context.TrailPhotos.RemoveRange(trail.Photos);  // Verwijder alle foto's van de trail
                }

                // Verwijder de favoriete trails die aan deze trail zijn gekoppeld
                if (trail.FavoriteTrails != null)
                {
                    _context.FavoriteTrails.RemoveRange(trail.FavoriteTrails);  // Verwijder de favoriete trails
                }

                // Verwijder de trail zelf
                _context.Trails.Remove(trail);

                await _context.SaveChangesAsync();
            }
        }




    }
}
