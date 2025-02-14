using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string pattern = @"^(?=.*[A-Z])(?=(.*[\W_]){2,})(?=.*05)(?=(.*[Faizan]){4,})[A-Za-z0-9\W_]{8,12}$";
        string[] testPasswords = {
            "05F@zan#",
            "Fai05@n!",
            "FaizAn06$#",
            "06Aa@bS_zo"
        };

        foreach (string password in testPasswords)
        {
            Console.WriteLine($"{password}: {Regex.IsMatch(password, pattern)}");
        }
    }
}
