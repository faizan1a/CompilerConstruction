using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string pattern = @"(\&\&|\|\||\!|\=\=|\!\=|\<|\>|\<\=|\>\=)";

        Console.WriteLine("Enter a string to search for logical operators:");
        string input = Console.ReadLine();

       Regex regex = new Regex(pattern);

        MatchCollection matches = regex.Matches(input);

        if (matches.Count > 0)
        {
            Console.WriteLine("Logical operators found:");
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Value);
            }
        }
        else
        {
            Console.WriteLine("No logical operators found.");
        }
    }
}
