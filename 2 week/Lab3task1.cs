using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
       string pattern = @"^[+-]?\d{0,4}(\.\d{1,2})?$";

        Console.WriteLine("Enter a floating-point number (length ≤ 6):");
        string input = Console.ReadLine();

        Regex regex = new Regex(pattern);

        if (regex.IsMatch(input))
        {
            Console.WriteLine("Valid floating-point number.");
        }
        else
        {
            Console.WriteLine("Invalid floating-point number.");
        }
    }
}
