using Microsoft.EntityFrameworkCore;
using PeaktrailsApp.Data.Entities;
using PeaktrailsBackend.Data;

namespace PeaktrailsApp.Data
{
    public class UsersRepository
    {
        private readonly PeaktrailsAppContext _context;

        public UsersRepository(PeaktrailsAppContext context)
        {
            _context = context;
        }

        // Haal alle gebruikers op, inclusief hun favoriete trails
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users
                .Include(u => u.FavoriteTrails) // Inclusief favorieten
                .ThenInclude(ft => ft.Trail) // Inclusief traildetails
                .ToListAsync();
        }

        // Haal een specifieke gebruiker op via ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.FavoriteTrails)
                .ThenInclude(ft => ft.Trail)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Voeg een nieuwe gebruiker toe
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Trail>> GetFavoriteTrailsByUserIdAsync(int userId)
        {
            return await _context.FavoriteTrails
                .Where(ft => ft.UserId == userId)
                .Include(ft => ft.Trail) // Zorg ervoor dat de Trail zelf mee wordt geladen
                .ThenInclude(t => t.Photos)
                .Select(ft => ft.Trail) // Retourneer alleen de Trail
                .ToListAsync();
        }

        // Voeg een trail toe aan de favorieten van een gebruiker
        public async Task AddFavoriteTrailAsync(int userId, int trailId)
        {
            var favoriteTrail = new FavoriteTrail
            {
                UserId = userId,
                TrailId = trailId
            };
            _context.FavoriteTrails.Add(favoriteTrail);
            await _context.SaveChangesAsync();
        }
    }
}
