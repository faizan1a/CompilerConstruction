using System;
using System.Collections.Generic;
using System.Linq;

namespace TopDownParser
{
    public class FirstFollowCalculator
    {
        public Dictionary<string, List<string[]>> grammar;
        private Dictionary<string, HashSet<string>> firstSets;
        private Dictionary<string, HashSet<string>> followSets;
        private HashSet<string> terminals;
        private HashSet<string> nonTerminals;
        private string startSymbol;
        private const string EPSILON = "ε";
        private const string END_MARKER = "$";

        public FirstFollowCalculator()
        {
            grammar = new Dictionary<string, List<string[]>>();
            firstSets = new Dictionary<string, HashSet<string>>();
            followSets = new Dictionary<string, HashSet<string>>();
            terminals = new HashSet<string>();
            nonTerminals = new HashSet<string>();
            startSymbol = "";
        }

        public void AddProduction(string leftSide, string[] rightSide)
        {
            if (!grammar.ContainsKey(leftSide))
            {
                grammar[leftSide] = new List<string[]>();
            }
            grammar[leftSide].Add(rightSide);

            // Set start symbol as the first non-terminal added
            if (string.IsNullOrEmpty(startSymbol))
            {
                startSymbol = leftSide;
            }

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

        public void CalculateFollowSets()
        {
            // Initialize FOLLOW sets
            foreach (string nonTerminal in nonTerminals)
            {
                followSets[nonTerminal] = new HashSet<string>();
            }

            // Rule 1: Add $ to FOLLOW(start symbol)
            if (!string.IsNullOrEmpty(startSymbol))
            {
                followSets[startSymbol].Add(END_MARKER);
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
                        for (int i = 0; i < rightSide.Length; i++)
                        {
                            string currentSymbol = rightSide[i];

                            // Only process non-terminals
                            if (!nonTerminals.Contains(currentSymbol))
                                continue;

                            // Rule 2: If A → αBβ, add FIRST(β) - {ε} to FOLLOW(B)
                            if (i + 1 < rightSide.Length)
                            {
                                string[] beta = rightSide.Skip(i + 1).ToArray();
                                HashSet<string> firstOfBeta = CalculateFirstOfProduction(beta);

                                int oldCount = followSets[currentSymbol].Count;
                                followSets[currentSymbol].UnionWith(firstOfBeta.Where(s => s != EPSILON));

                                if (followSets[currentSymbol].Count > oldCount)
                                {
                                    changed = true;
                                }

                                // Rule 3: If A → αBβ and ε ∈ FIRST(β), add FOLLOW(A) to FOLLOW(B)
                                if (firstOfBeta.Contains(EPSILON))
                                {
                                    oldCount = followSets[currentSymbol].Count;
                                    followSets[currentSymbol].UnionWith(followSets[leftSide]);

                                    if (followSets[currentSymbol].Count > oldCount)
                                    {
                                        changed = true;
                                    }
                                }
                            }
                            else
                            {
                                // Rule 3: If A → αB, add FOLLOW(A) to FOLLOW(B)
                                int oldCount = followSets[currentSymbol].Count;
                                followSets[currentSymbol].UnionWith(followSets[leftSide]);

                                if (followSets[currentSymbol].Count > oldCount)
                                {
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DisplayGrammar()
        {
            Console.WriteLine("\n=== GRAMMAR PRODUCTIONS ===");
            Console.WriteLine($"Start Symbol: {startSymbol}");
            Console.WriteLine($"Non-Terminals: {{ {string.Join(", ", nonTerminals.OrderBy(x => x))} }}");
            Console.WriteLine($"Terminals: {{ {string.Join(", ", terminals.OrderBy(x => x))} }}");
            Console.WriteLine();

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

        public void DisplayFirstSets()
        {
            Console.WriteLine("\n=== FIRST SETS ===");
            foreach (var kvp in firstSets.OrderBy(x => x.Key))
            {
                Console.WriteLine($"FIRST({kvp.Key}) = {{ {string.Join(", ", kvp.Value.OrderBy(x => x))} }}");
            }
        }

        public void DisplayFollowSets()
        {
            Console.WriteLine("\n=== FOLLOW SETS ===");
            foreach (var kvp in followSets.OrderBy(x => x.Key))
            {
                Console.WriteLine($"FOLLOW({kvp.Key}) = {{ {string.Join(", ", kvp.Value.OrderBy(x => x))} }}");
            }
        }

        public void DisplayParsingTable()
        {
            Console.WriteLine("\n=== LL(1) PARSING TABLE (Partial) ===");
            Console.WriteLine("Non-Terminal\tTerminal\tProduction");
            Console.WriteLine(new string('-', 50));

            foreach (var production in grammar)
            {
                string leftSide = production.Key;

                for (int prodIndex = 0; prodIndex < production.Value.Count; prodIndex++)
                {
                    string[] rightSide = production.Value[prodIndex];
                    HashSet<string> firstOfProduction = CalculateFirstOfProduction(rightSide);

                    // For each terminal in FIRST(production)
                    foreach (string terminal in firstOfProduction.Where(t => t != EPSILON))
                    {
                        Console.WriteLine($"{leftSide}\t\t{terminal}\t\t{leftSide} → {string.Join(" ", rightSide)}");
                    }

                    // If epsilon in FIRST(production), add entries for FOLLOW(leftSide)
                    if (firstOfProduction.Contains(EPSILON))
                    {
                        foreach (string followTerminal in followSets[leftSide])
                        {
                            Console.WriteLine($"{leftSide}\t\t{followTerminal}\t\t{leftSide} → {string.Join(" ", rightSide)}");
                        }
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Top-Down Parser: FIRST and FOLLOW Set Calculator");
            Console.WriteLine("================================================");

            // Example 1: Simple Grammar with 4 Non-Terminals and 4 Terminals
            Console.WriteLine("\nExample 1: Simple Grammar (4 Non-Terminals, 4+ Terminals)");
            FirstFollowCalculator calculator1 = new FirstFollowCalculator();
            SetupSimpleGrammar(calculator1);

            calculator1.DisplayGrammar();
            calculator1.CalculateFirstSets();
            calculator1.DisplayFirstSets();
            calculator1.CalculateFollowSets();
            calculator1.DisplayFollowSets();
            calculator1.DisplayParsingTable();

            // Example 2: Expression Grammar
            Console.WriteLine("\n\n" + new string('=', 60));
            Console.WriteLine("Example 2: Expression Grammar");
            FirstFollowCalculator calculator2 = new FirstFollowCalculator();
            SetupExpressionGrammar(calculator2);

            calculator2.DisplayGrammar();
            calculator2.CalculateFirstSets();
            calculator2.DisplayFirstSets();
            calculator2.CalculateFollowSets();
            calculator2.DisplayFollowSets();

            // Example 3: Programming Language Grammar
            Console.WriteLine("\n\n" + new string('=', 60));
            Console.WriteLine("Example 3: Programming Language Grammar");
            FirstFollowCalculator calculator3 = new FirstFollowCalculator();
            SetupProgrammingGrammar(calculator3);

            calculator3.DisplayGrammar();
            calculator3.CalculateFirstSets();
            calculator3.DisplayFirstSets();
            calculator3.CalculateFollowSets();
            calculator3.DisplayFollowSets();

            // Interactive mode
            Console.WriteLine("\n\n" + new string('=', 60));
            Console.WriteLine("Interactive Mode - Enter your own grammar:");
            InteractiveMode();
        }

        static void SetupSimpleGrammar(FirstFollowCalculator calculator)
        {
            // Simple grammar with exactly 4 Non-Terminals (S, A, B, C) and 4+ Terminals (a, b, c, d)
            // S → A B
            // A → a A | ε
            // B → b C
            // C → c C | d

            calculator.AddProduction("S", new string[] { "A", "B" });
            calculator.AddProduction("A", new string[] { "a", "A" });
            calculator.AddProduction("A", new string[] { "ε" });
            calculator.AddProduction("B", new string[] { "b", "C" });
            calculator.AddProduction("C", new string[] { "c", "C" });
            calculator.AddProduction("C", new string[] { "d" });
        }

        static void SetupExpressionGrammar(FirstFollowCalculator calculator)
        {
            // Expression grammar with 4 Non-Terminals: E, E', T, T', F
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

        static void SetupProgrammingGrammar(FirstFollowCalculator calculator)
        {
            // Programming language grammar with 4+ Non-Terminals and 4+ Terminals
            // Program → Stmt
            // Stmt → IfStmt | AssignStmt | Block
            // IfStmt → if ( Expr ) Stmt
            // AssignStmt → id = Expr ;
            // Block → { StmtList }
            // StmtList → Stmt StmtList | ε
            // Expr → id | num

            calculator.AddProduction("Program", new string[] { "Stmt" });
            calculator.AddProduction("Stmt", new string[] { "IfStmt" });
            calculator.AddProduction("Stmt", new string[] { "AssignStmt" });
            calculator.AddProduction("Stmt", new string[] { "Block" });
            calculator.AddProduction("IfStmt", new string[] { "if", "(", "Expr", ")", "Stmt" });
            calculator.AddProduction("AssignStmt", new string[] { "id", "=", "Expr", ";" });
            calculator.AddProduction("Block", new string[] { "{", "StmtList", "}" });
            calculator.AddProduction("StmtList", new string[] { "Stmt", "StmtList" });
            calculator.AddProduction("StmtList", new string[] { "ε" });
            calculator.AddProduction("Expr", new string[] { "id" });
            calculator.AddProduction("Expr", new string[] { "num" });
        }

        static void InteractiveMode()
        {
            FirstFollowCalculator calculator = new FirstFollowCalculator();

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
                calculator.CalculateFollowSets();
                calculator.DisplayFollowSets();
                calculator.DisplayParsingTable();
            }
            else
            {
                Console.WriteLine("No productions entered.");
            }
        }

        static void ParseAndAddProduction(FirstFollowCalculator calculator, string input)
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