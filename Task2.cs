using System;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        // Inputs
        Console.WriteLine("Enter your first name:");
        string firstName = Console.ReadLine();

        Console.WriteLine("Enter your last name:");
        string lastName = Console.ReadLine();

        Console.WriteLine("Enter your registration number (3 digits only):");
        string regNumber = Console.ReadLine();

        Console.WriteLine("Enter your favorite movie:");
        string favoriteMovie = Console.ReadLine();

        Console.WriteLine("Enter your favorite food:");
        string favoriteFood = Console.ReadLine();

        // Generate password
        string password = GeneratePassword(firstName, lastName, regNumber, favoriteMovie, favoriteFood);

        Console.WriteLine($"Your generated password is: {password}");
    }

    static string GeneratePassword(string firstName, string lastName, string regNumber, string favoriteMovie, string favoriteFood)
    {
        // Extract parts of inputs
        string part1 = firstName.Substring(0, Math.Min(2, firstName.Length)); // First 2 letters of first name
        string part2 = lastName.Substring(0, Math.Min(2, lastName.Length));   // First 2 letters of last name
        string part3 = regNumber;                                             // Registration number
        string part4 = favoriteMovie.Substring(0, Math.Min(2, favoriteMovie.Length)); // First 2 letters of favorite movie
        string part5 = favoriteFood.Substring(0, Math.Min(2, favoriteFood.Length));   // First 2 letters of favorite food

        // Combine parts
        string combined = part1 + part2 + part3 + part4 + part5;

        // Add randomness
        Random random = new Random();
        string specialChars = "!@#$%^&*()_+{}|:\"<>?~`-=[]\\;',./";
        string numbers = "0123456789";
        string uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

        // Add one random uppercase letter
        combined += uppercaseLetters[random.Next(uppercaseLetters.Length)];

        // Add one random lowercase letter
        combined += lowercaseLetters[random.Next(lowercaseLetters.Length)];

        // Add one random special character
        combined += specialChars[random.Next(specialChars.Length)];

        // Add one random number
        combined += numbers[random.Next(numbers.Length)];

        // Shuffle the combined string to make it more random
        string shuffledPassword = new string(combined.OrderBy(c => random.Next()).ToArray());

        // Ensure the password is not longer than 12 characters
        if (shuffledPassword.Length > 12)
        {
            shuffledPassword = shuffledPassword.Substring(0, 12);
        }

        return shuffledPassword;
    }
}
