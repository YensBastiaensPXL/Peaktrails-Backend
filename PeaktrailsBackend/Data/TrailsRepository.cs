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

        public async Task DeleteTrailAsync(int id) // Wijziging hier
        {
            var trail = await _context.Trails.FindAsync(id); // Wijziging hier
            if (trail != null)
            {
                _context.Trails.Remove(trail); // Wijziging hier
                await _context.SaveChangesAsync();
            }
        }
    }
}
