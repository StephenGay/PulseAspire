// Pulse.ApiService\Services\PasswordGeneratorService.cs
using System.Security.Cryptography;

namespace Pulse.ApiService.Services
{
    public interface IPasswordGeneratorService
    {
        string GenerateRandomPassword(int length = 12);
    }

    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        public string GenerateRandomPassword(int length = 12)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

            var allChars = UpperCase + LowerCase + Numbers + SpecialChars;
            var password = new char[length];

            // Ensure at least one character from each category
            password[0] = GetRandomChar(UpperCase);
            password[1] = GetRandomChar(LowerCase);
            password[2] = GetRandomChar(Numbers);
            password[3] = GetRandomChar(SpecialChars);

            // Fill remaining positions randomly
            for (int i = 4; i < length; i++)
            {
                password[i] = GetRandomChar(allChars);
            }

            // Shuffle the password
            return ShufflePassword(password);
        }

        private char GetRandomChar(string chars)
        {
            var randomIndex = RandomNumberGenerator.GetInt32(chars.Length);
            return chars[randomIndex];
        }

        private string ShufflePassword(char[] password)
        {
            for (int i = password.Length - 1; i > 0; i--)
            {
                int randomIndex = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[randomIndex]) = (password[randomIndex], password[i]);
            }

            return new string(password);
        }
    }
}