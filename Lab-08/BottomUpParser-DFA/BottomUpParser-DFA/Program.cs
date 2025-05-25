using System;
using System.Collections.Generic;
using System.Linq;

namespace BottomUpParser
{
    public class DFAState
    {
        public int StateNumber { get; set; }
        public bool IsAccepting { get; set; }
        public string Description { get; set; }
        public Dictionary<char, int> Transitions { get; set; }

        public DFAState(int stateNumber, bool isAccepting, string description)
        {
            StateNumber = stateNumber;
            IsAccepting = isAccepting;
            Description = description;
            Transitions = new Dictionary<char, int>();
        }

        public void AddTransition(char input, int nextState)
        {
            Transitions[input] = nextState;
        }

        public int GetNextState(char input)
        {
            return Transitions.ContainsKey(input) ? Transitions[input] : -1;
        }
    }

    public class DeterministicFiniteAutomaton
    {
        private Dictionary<int, DFAState> states;
        private int startState;
        private int currentState;
        private int deadState;

        public DeterministicFiniteAutomaton()
        {
            states = new Dictionary<int, DFAState>();
            startState = 0;
            currentState = startState;
            deadState = -1;
            InitializeCVariableDFA();
        }

        private void InitializeCVariableDFA()
        {
            // State 0: Start state - expecting letter or underscore
            DFAState state0 = new DFAState(0, false, "Start - Expecting letter or underscore");

            // State 1: Accepting state - valid variable name
            DFAState state1 = new DFAState(1, true, "Valid variable - Can accept more letters, digits, or underscores");

            // State 2: Dead state - invalid input
            DFAState state2 = new DFAState(2, false, "Dead state - Invalid variable name");

            states[0] = state0;
            states[1] = state1;
            states[2] = state2;

            // Define transitions for State 0 (Start)
            // Letters (a-z, A-Z) and underscore (_) go to State 1
            for (char c = 'a'; c <= 'z'; c++)
                state0.AddTransition(c, 1);
            for (char c = 'A'; c <= 'Z'; c++)
                state0.AddTransition(c, 1);
            state0.AddTransition('_', 1);

            // Define transitions for State 1 (Accepting)
            // Letters, digits, and underscore stay in State 1
            for (char c = 'a'; c <= 'z'; c++)
                state1.AddTransition(c, 1);
            for (char c = 'A'; c <= 'Z'; c++)
                state1.AddTransition(c, 1);
            for (char c = '0'; c <= '9'; c++)
                state1.AddTransition(c, 1);
            state1.AddTransition('_', 1);

            // State 2 (Dead state) has no outgoing transitions
            // Any input keeps it in the dead state
        }

        public bool AcceptString(string input)
        {
            Reset();

            if (string.IsNullOrEmpty(input))
                return false;

            foreach (char c in input)
            {
                ProcessInput(c);
                if (currentState == deadState || currentState == 2)
                {
                    return false;
                }
            }

            return states[currentState].IsAccepting;
        }

        private void ProcessInput(char input)
        {
            int nextState = states[currentState].GetNextState(input);

            if (nextState == -1)
            {
                // No valid transition - go to dead state
                currentState = 2;
            }
            else
            {
                currentState = nextState;
            }
        }

        public void Reset()
        {
            currentState = startState;
        }

        public void DisplayDFA()
        {
            Console.WriteLine("=== DETERMINISTIC FINITE AUTOMATON FOR C VARIABLES ===");
            Console.WriteLine();
            Console.WriteLine("States:");
            foreach (var state in states.Values.OrderBy(s => s.StateNumber))
            {
                string acceptingStatus = state.IsAccepting ? "(Accepting)" : "";
                string startStatus = (state.StateNumber == startState) ? "(Start)" : "";
                Console.WriteLine($"  q{state.StateNumber} {startStatus}{acceptingStatus}: {state.Description}");
            }

            Console.WriteLine();
            Console.WriteLine("Transition Function:");
            Console.WriteLine("State\tInput\t\tNext State");
            Console.WriteLine(new string('-', 40));

            foreach (var state in states.Values.OrderBy(s => s.StateNumber))
            {
                var groupedTransitions = GroupTransitions(state.Transitions);

                foreach (var group in groupedTransitions)
                {
                    Console.WriteLine($"q{state.StateNumber}\t{group.Key}\t\tq{group.Value}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Language: L = {w | w is a valid C variable name}");
            Console.WriteLine("Rules:");
            Console.WriteLine("  1. Must start with a letter (a-z, A-Z) or underscore (_)");
            Console.WriteLine("  2. Followed by any combination of letters, digits (0-9), or underscores");
            Console.WriteLine("  3. Cannot be empty");
            Console.WriteLine("  4. Cannot start with a digit");
        }

        private Dictionary<string, int> GroupTransitions(Dictionary<char, int> transitions)
        {
            var grouped = new Dictionary<string, int>();
            var stateGroups = transitions.GroupBy(t => t.Value);

            foreach (var stateGroup in stateGroups)
            {
                var chars = stateGroup.Select(t => t.Key).OrderBy(c => c).ToList();
                string key = FormatCharacterGroup(chars);
                grouped[key] = stateGroup.Key;
            }

            return grouped;
        }

        private string FormatCharacterGroup(List<char> chars)
        {
            if (chars.Count == 0) return "";
            if (chars.Count == 1) return chars[0].ToString();

            // Group consecutive characters
            var groups = new List<string>();
            int start = 0;

            for (int i = 1; i <= chars.Count; i++)
            {
                if (i == chars.Count || chars[i] != chars[i - 1] + 1)
                {
                    if (i - start == 1)
                    {
                        groups.Add(chars[start].ToString());
                    }
                    else if (i - start == 2)
                    {
                        groups.Add($"{chars[start]}, {chars[i - 1]}");
                    }
                    else
                    {
                        groups.Add($"{chars[start]}-{chars[i - 1]}");
                    }
                    start = i;
                }
            }

            return string.Join(", ", groups);
        }

        public void TestString(string input)
        {
            Console.WriteLine($"\nTesting: \"{input}\"");
            Console.WriteLine(new string('-', 30));

            Reset();
            bool stepByStep = input.Length <= 10; // Show step-by-step for short strings

            if (stepByStep)
            {
                Console.WriteLine($"Initial state: q{currentState} ({states[currentState].Description})");
            }

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Empty string - REJECTED");
                return;
            }

            foreach (char c in input)
            {
                int prevState = currentState;
                ProcessInput(c);

                if (stepByStep)
                {
                    if (currentState == 2)
                    {
                        Console.WriteLine($"Input '{c}': q{prevState} → q{currentState} (Dead state)");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Input '{c}': q{prevState} → q{currentState}");
                    }
                }

                if (currentState == 2)
                    break;
            }

            bool accepted = states[currentState].IsAccepting;
            string result = accepted ? "ACCEPTED" : "REJECTED";
            string reason = "";

            if (!accepted)
            {
                if (currentState == 2)
                    reason = " (Invalid character or starts with digit)";
                else if (currentState == 0)
                    reason = " (Empty string)";
            }

            Console.WriteLine($"Final state: q{currentState}");
            Console.WriteLine($"Result: {result}{reason}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bottom-Up Parser: Deterministic Finite Automaton");
            Console.WriteLine("Implementation for C Variable Recognition");
            Console.WriteLine(new string('=', 55));

            DeterministicFiniteAutomaton dfa = new DeterministicFiniteAutomaton();

            // Display DFA structure
            dfa.DisplayDFA();

            // Test cases
            Console.WriteLine("\n" + new string('=', 55));
            Console.WriteLine("TEST CASES");
            Console.WriteLine(new string('=', 55));

            string[] testCases = {
                // Valid C variables
                "x",
                "var",
                "Variable",
                "_temp",
                "var1",
                "my_variable",
                "_123",
                "camelCase",
                "CONSTANT",
                "_private_var",
                "var_123_abc",
                
                // Invalid C variables
                "",              // Empty string
                "1var",          // Starts with digit
                "123",           // All digits
                "var-name",      // Contains hyphen
                "my variable",   // Contains space
                "var@name",      // Contains special character
                "var.name",      // Contains dot
                "var+name",      // Contains plus
                "var#name"       // Contains hash
            };

            foreach (string testCase in testCases)
            {
                dfa.TestString(testCase);
            }

            // Interactive testing
            Console.WriteLine("\n" + new string('=', 55));
            Console.WriteLine("INTERACTIVE TESTING");
            Console.WriteLine(new string('=', 55));
            InteractiveTest(dfa);
        }

        static void InteractiveTest(DeterministicFiniteAutomaton dfa)
        {
            Console.WriteLine("Enter variable names to test (type 'quit' to exit):");

            while (true)
            {
                Console.Write("\nEnter variable name: ");
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) || input.ToLower() == "quit")
                    break;

                dfa.TestString(input);

                // Quick result
                bool result = dfa.AcceptString(input);
                Console.WriteLine($"Quick check: {(result ? "✓ VALID" : "✗ INVALID")} C variable");
            }

            Console.WriteLine("\nThank you for using the DFA C Variable Recognizer!");
        }
    }
}