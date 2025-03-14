using System;

class LL1Parser
{
    private string input;
    private int index;

    public LL1Parser(string input)
    {
        this.input = input;
        this.index = 0;
    }

    private char Lookahead()
    {
        return index < input.Length ? input[index] : '\0';
    }

    private void Match(char expected)
    {
        if (Lookahead() == expected)
            index++;
        else
            throw new Exception($"Syntax error: expected '{expected}' but found '{Lookahead()}'");
    }

    public void Parse()
    {
        S();
        if (index != input.Length)
            throw new Exception("Syntax error: unexpected characters at end of input");
        Console.WriteLine("Input successfully parsed.");
    }

    private void S()
    {
        if (Lookahead() == '(')
        {
            Match('(');
            C();
            if (Lookahead() == 'x')
                Match('x');
            else 
                throw new Exception("Syntax error in S");
            if (Lookahead() == 'y')
                Match('y');
            else 
                throw new Exception("Syntax error in S");
        }
        else if (Lookahead() == 'd')
        {
            Match('d');
            if (Lookahead() == 'y')
                Match('y');
            else 
                throw new Exception("Syntax error in S");
        }
        else if (Lookahead() == 'b')
        {
            Match('b');
        }
        else
        {
            throw new Exception("Syntax error in S");
        }
        SPrime();
    }

    private void SPrime()
    {
        if (Lookahead() == 'x')
        {
            Match('x');
            Match('y');
            SPrime();
        }
        // ε case (do nothing)
    }

    private void C()
    {
        if (Lookahead() == 'e')
        {
            Match('e');
            CPrime();
        }
        else
        {
            throw new Exception("Syntax error in C");
        }
    }

    private void CPrime()
    {
        if (Lookahead() == 'm')
        {
            Match('m');
            CPrime();
        }
        // ε case (do nothing)
    }

    public static void Main()
    {
        Console.Write("Enter input string: ");
        string input = Console.ReadLine();
        try
        {
            LL1Parser parser = new LL1Parser(input);
            parser.Parse();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
