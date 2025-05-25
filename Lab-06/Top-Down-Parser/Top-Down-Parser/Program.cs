using System;
using System.Collections.Generic;
using System.Linq;

namespace TopDownParser
{
    public class FirstSetCalculator
    {
        public Dictionary<string, List<string[]>> grammar;
        private Dictionary<string, HashSet<string>> firstSets;
        private HashSet<string> terminals;
        private HashSet<string> nonTerminals;
        private const string EPSILON = "ε";

        public FirstSetCalculator()
        {
            grammar = new Dictionary<string, List<string[]>>();
            firstSets = new Dictionary<string, HashSet<string>>();
            terminals = new HashSet<string>();
            nonTerminals = new HashSet<string>();
        }

        public void AddProduction(string leftSide, string[] rightSide)
        {
            if (!grammar.ContainsKey(leftSide))
            {
                grammar[leftSide] = new List<string[]>();
            }
            grammar[leftSide].Add(rightSide);

            // Add to non-terminals
            nonTerminals.Add(leftSide);

            // Identify terminals and non-terminals from right side
            foreach (string symbol in rightSide)
            {
                if (symbol != EPSILON && !IsNonTerminal(symbol))
                {
                    terminals.Add(symbol);
                }
            }
        }

        private bool IsNonTerminal(string symbol)
        {
            return grammar.ContainsKey(symbol);
        }

        public void CalculateFirstSets()
        {
            // Initialize FIRST sets
            foreach (string nonTerminal in nonTerminals)
            {
                firstSets[nonTerminal] = new HashSet<string>();
            }

            bool changed = true;
            while (changed)
            {
                changed = false;

                foreach (var production in grammar)
                {
                    string leftSide = production.Key;

                    foreach (string[] rightSide in production.Value)
                    {
                        HashSet<string> firstOfProduction = CalculateFirstOfProduction(rightSide);

                        int oldCount = firstSets[leftSide].Count;
                        firstSets[leftSide].UnionWith(firstOfProduction);

                        if (firstSets[leftSide].Count > oldCount)
                        {
                            changed = true;
                        }
                    }
                }
            }
        }

        private HashSet<string> CalculateFirstOfProduction(string[] production)
        {
            HashSet<string> result = new HashSet<string>();

            for (int i = 0; i < production.Length; i++)
            {
                string symbol = production[i];

                if (symbol == EPSILON)
                {
                    result.Add(EPSILON);
                    break;
                }
                else if (terminals.Contains(symbol))
                {
                    result.Add(symbol);
                    break;
                }
                else if (nonTerminals.Contains(symbol))
                {
                    HashSet<string> firstOfSymbol = new HashSet<string>(firstSets[symbol]);
                    result.UnionWith(firstOfSymbol.Where(s => s != EPSILON));

                    if (!firstOfSymbol.Contains(EPSILON))
                    {
                        break;
                    }

                    // If we've processed all symbols and all can derive epsilon
                    if (i == production.Length - 1)
                    {
                        result.Add(EPSILON);
                    }
                }
            }

            return result;
        }

        public void DisplayFirstSets()
        {
            Console.WriteLine("\n=== FIRST SETS ===");
            foreach (var kvp in firstSets.OrderBy(x => x.Key))
            {
                Console.WriteLine($"FIRST({kvp.Key}) = {{ {string.Join(", ", kvp.Value.OrderBy(x => x))} }}");
            }
        }

        public void DisplayGrammar()
        {
            Console.WriteLine("\n=== GRAMMAR PRODUCTIONS ===");
            foreach (var production in grammar)
            {
                string leftSide = production.Key;
                for (int i = 0; i < production.Value.Count; i++)
                {
                    string arrow = (i == 0) ? "→" : " |";
                    Console.WriteLine($"{leftSide} {arrow} {string.Join(" ", production.Value[i])}");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Top-Down Parser: FIRST Set Calculator");
            Console.WriteLine("=====================================");

            FirstSetCalculator calculator = new FirstSetCalculator();

            // Example 1: Simple expression grammar
            Console.WriteLine("\nExample 1: Simple Expression Grammar");
            SetupExampleGrammar1(calculator);

            calculator.DisplayGrammar();
            calculator.CalculateFirstSets();
            calculator.DisplayFirstSets();

            // Example 2: Java-like language constructs
            Console.WriteLine("\n\n" + new string('=', 50));
            Console.WriteLine("Example 2: Java-like Language Constructs");
            calculator = new FirstSetCalculator();
            SetupJavaLikeGrammar(calculator);

            calculator.DisplayGrammar();
            calculator.CalculateFirstSets();
            calculator.DisplayFirstSets();

            // Interactive mode
            Console.WriteLine("\n\n" + new string('=', 50));
            Console.WriteLine("Interactive Mode - Enter your own grammar:");
            InteractiveMode();
        }

        static void SetupExampleGrammar1(FirstSetCalculator calculator)
        {
            // Grammar:
            // E → T E'
            // E' → + T E' | ε
            // T → F T'
            // T' → * F T' | ε
            // F → ( E ) | id

            calculator.AddProduction("E", new string[] { "T", "E'" });
            calculator.AddProduction("E'", new string[] { "+", "T", "E'" });
            calculator.AddProduction("E'", new string[] { "ε" });
            calculator.AddProduction("T", new string[] { "F", "T'" });
            calculator.AddProduction("T'", new string[] { "*", "F", "T'" });
            calculator.AddProduction("T'", new string[] { "ε" });
            calculator.AddProduction("F", new string[] { "(", "E", ")" });
            calculator.AddProduction("F", new string[] { "id" });
        }

        static void SetupJavaLikeGrammar(FirstSetCalculator calculator)
        {
            // Java-like language constructs grammar:
            // Program → ClassDecl
            // ClassDecl → class id { MemberList }
            // MemberList → Member MemberList | ε
            // Member → MethodDecl | VarDecl
            // MethodDecl → Type id ( ParamList ) { StmtList }
            // VarDecl → Type id ;
            // ParamList → Param ParamTail | ε
            // ParamTail → , Param ParamTail | ε
            // Param → Type id
            // StmtList → Stmt StmtList | ε
            // Stmt → AssignStmt | IfStmt | WhileStmt | ReturnStmt | VarDecl
            // AssignStmt → id = Expr ;
            // IfStmt → if ( Expr ) { StmtList }
            // WhileStmt → while ( Expr ) { StmtList }
            // ReturnStmt → return Expr ;
            // Expr → Term ExprTail
            // ExprTail → + Term ExprTail | - Term ExprTail | ε
            // Term → Factor TermTail
            // TermTail → * Factor TermTail | / Factor TermTail | ε
            // Factor → id | num | ( Expr )
            // Type → int | boolean | void

            calculator.AddProduction("Program", new string[] { "ClassDecl" });
            calculator.AddProduction("ClassDecl", new string[] { "class", "id", "{", "MemberList", "}" });
            calculator.AddProduction("MemberList", new string[] { "Member", "MemberList" });
            calculator.AddProduction("MemberList", new string[] { "ε" });
            calculator.AddProduction("Member", new string[] { "MethodDecl" });
            calculator.AddProduction("Member", new string[] { "VarDecl" });
            calculator.AddProduction("MethodDecl", new string[] { "Type", "id", "(", "ParamList", ")", "{", "StmtList", "}" });
            calculator.AddProduction("VarDecl", new string[] { "Type", "id", ";" });
            calculator.AddProduction("ParamList", new string[] { "Param", "ParamTail" });
            calculator.AddProduction("ParamList", new string[] { "ε" });
            calculator.AddProduction("ParamTail", new string[] { ",", "Param", "ParamTail" });
            calculator.AddProduction("ParamTail", new string[] { "ε" });
            calculator.AddProduction("Param", new string[] { "Type", "id" });
            calculator.AddProduction("StmtList", new string[] { "Stmt", "StmtList" });
            calculator.AddProduction("StmtList", new string[] { "ε" });
            calculator.AddProduction("Stmt", new string[] { "AssignStmt" });
            calculator.AddProduction("Stmt", new string[] { "IfStmt" });
            calculator.AddProduction("Stmt", new string[] { "WhileStmt" });
            calculator.AddProduction("Stmt", new string[] { "ReturnStmt" });
            calculator.AddProduction("Stmt", new string[] { "VarDecl" });
            calculator.AddProduction("AssignStmt", new string[] { "id", "=", "Expr", ";" });
            calculator.AddProduction("IfStmt", new string[] { "if", "(", "Expr", ")", "{", "StmtList", "}" });
            calculator.AddProduction("WhileStmt", new string[] { "while", "(", "Expr", ")", "{", "StmtList", "}" });
            calculator.AddProduction("ReturnStmt", new string[] { "return", "Expr", ";" });
            calculator.AddProduction("Expr", new string[] { "Term", "ExprTail" });
            calculator.AddProduction("ExprTail", new string[] { "+", "Term", "ExprTail" });
            calculator.AddProduction("ExprTail", new string[] { "-", "Term", "ExprTail" });
            calculator.AddProduction("ExprTail", new string[] { "ε" });
            calculator.AddProduction("Term", new string[] { "Factor", "TermTail" });
            calculator.AddProduction("TermTail", new string[] { "*", "Factor", "TermTail" });
            calculator.AddProduction("TermTail", new string[] { "/", "Factor", "TermTail" });
            calculator.AddProduction("TermTail", new string[] { "ε" });
            calculator.AddProduction("Factor", new string[] { "id" });
            calculator.AddProduction("Factor", new string[] { "num" });
            calculator.AddProduction("Factor", new string[] { "(", "Expr", ")" });
            calculator.AddProduction("Type", new string[] { "int" });
            calculator.AddProduction("Type", new string[] { "boolean" });
            calculator.AddProduction("Type", new string[] { "void" });
        }

        static void InteractiveMode()
        {
            FirstSetCalculator calculator = new FirstSetCalculator();

            Console.WriteLine("Enter grammar productions (format: A -> B C D or A -> ε)");
            Console.WriteLine("Type 'done' when finished entering productions");
            Console.WriteLine("Use 'ε' for epsilon (empty production)");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter production: ");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "done")
                    break;

                try
                {
                    ParseAndAddProduction(calculator, input);
                    Console.WriteLine("✓ Production added successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                    Console.WriteLine("Please use format: A -> B C D");
                }
            }

            if (calculator.grammar.Count > 0)
            {
                calculator.DisplayGrammar();
                calculator.CalculateFirstSets();
                calculator.DisplayFirstSets();
            }
            else
            {
                Console.WriteLine("No productions entered.");
            }
        }

        static void ParseAndAddProduction(FirstSetCalculator calculator, string input)
        {
            string[] parts = input.Split(new string[] { "->", "→" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid production format. Use: A -> B C D");

            string leftSide = parts[0].Trim();
            string[] rightSide = parts[1].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (rightSide.Length == 0)
                rightSide = new string[] { "ε" };

            calculator.AddProduction(leftSide, rightSide);
        }
    }
}