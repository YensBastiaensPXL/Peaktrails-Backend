using System.Security.Cryptography;

public class PasswordHasher
{
    public static string HashPassword(string password)
    {
        // Genereer een salt
        byte[] salt = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }

        // Hash het wachtwoord met de salt en PBKDF2
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000); // 100,000 iteraties
        byte[] hash = pbkdf2.GetBytes(32); // Genereer een 256-bit hash

        // Combineer de salt en de hash
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16); // Eerste 16 bytes zijn de salt
        Array.Copy(hash, 0, hashBytes, 16, 32); // Laatste 32 bytes zijn de hash

        // Converteer naar een Base64-string voor opslag
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        // Decodeer de opgeslagen hash
        byte[] hashBytes = Convert.FromBase64String(storedHash);

        // Haal de salt (eerste 16 bytes) op
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Hash het ingevoerde wachtwoord opnieuw met dezelfde salt
        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(32);

        // Vergelijk de gegenereerde hash met de opgeslagen hash
        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false; // Mismatch, wachtwoord ongeldig
        }

        return true; // Wachtwoord klopt
    }
}
