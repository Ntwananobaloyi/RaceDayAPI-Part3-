namespace RaceDayAPI.Services 
{
    public static class PasswordHasher
    {
        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public static bool Verify(string password, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch { return false; } // malformed hash => treat as wrong password
        }
    }
}
