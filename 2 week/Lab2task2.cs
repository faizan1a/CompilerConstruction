using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string pattern = @"(\<\=|\>\=|\=\=|\!\=|\<|\>)";

        Console.WriteLine("Enter a string to search for relational operators:");
        string input = Console.ReadLine();

        Regex regex = new Regex(pattern);

         MatchCollection matches = regex.Matches(input);

        if (matches.Count > 0)
        {
            Console.WriteLine("Relational operators found:");
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Value);
            }
        }
        else
        {
            Console.WriteLine("No relational operators found.");
        }
    }
}
