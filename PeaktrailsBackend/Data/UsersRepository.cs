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

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Trail>> GetTrailsByUserIdAsync(int userId)
        {
            return await _context.Trails
                .Where(t => t.UserId == userId)
                .Include(t => t.Photos)  // Voorbeeld: trails ophalen inclusief hun foto's
                .ToListAsync();
        }

        public async Task<bool> DeleteUserTrailAsync(int userId, int trailId)
        {
            var trail = await _context.Trails
                .FirstOrDefaultAsync(t => t.UserId == userId && t.TrailId == trailId);

            if (trail == null)
            {
                return false; // Trail niet gevonden of niet gekoppeld aan de gebruiker
            }

            _context.Trails.Remove(trail);
            await _context.SaveChangesAsync();
            return true; // Trail succesvol verwijderd
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

        public async Task<bool> AddFavoriteTrailAsync(int userId, int trailId)
        {
            // Controleer of de trail al een favoriet is van deze gebruiker
            var existingFavorite = await _context.FavoriteTrails
                .FirstOrDefaultAsync(ft => ft.UserId == userId && ft.TrailId == trailId);

            if (existingFavorite != null)
            {
                // Als het al een favoriet is, return false of doe verder niets
                return false;
            }

            // Voeg de nieuwe favoriete trail toe
            var favoriteTrail = new FavoriteTrail
            {
                UserId = userId,
                TrailId = trailId
            };
            _context.FavoriteTrails.Add(favoriteTrail);
            await _context.SaveChangesAsync();

            return true; // Return true als het succesvol is toegevoegd
        }


        public async Task<bool> RemoveFavoriteTrailAsync(int userId, int trailId)
        {
            // Zoek naar de favoriete trail
            var favoriteTrail = await _context.FavoriteTrails
                .FirstOrDefaultAsync(ft => ft.UserId == userId && ft.TrailId == trailId);

            if (favoriteTrail == null)
            {
                return false; // Als de trail geen favoriet is van de gebruiker
            }

            // Verwijder de favoriete trail
            _context.FavoriteTrails.Remove(favoriteTrail);
            await _context.SaveChangesAsync();

            return true; // Return true als het succesvol is verwijderd
        }


    }
}
