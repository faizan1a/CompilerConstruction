using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LexicalAnalyzer
{
    // Token types enumeration
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        INTEGER,
        FLOAT,
        STRING,
        CHAR,
        OPERATOR,
        DELIMITER,
        ASSIGNMENT,
        COMMENT,
        WHITESPACE,
        NEWLINE,
        EOF,
        ERROR
    }

    // Token class representing a lexical unit
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public object Value { get; set; }

        public Token(TokenType type, string lexeme, int line, int column, object value = null)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            Value = value ?? lexeme;
        }

        public override string ToString()
        {
            return $"<{Type}, '{Lexeme}', {Line}:{Column}>";
        }
    }

    // Symbol table entry
    public class SymbolTableEntry
    {
        public string Name { get; set; }
        public TokenType Type { get; set; }
        public string DataType { get; set; }
        public object Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string Scope { get; set; }
        public bool IsInitialized { get; set; }

        public SymbolTableEntry(string name, TokenType type, string dataType, int line, int column, string scope = "global")
        {
            Name = name;
            Type = type;
            DataType = dataType;
            Line = line;
            Column = column;
            Scope = scope;
            IsInitialized = false;
            Value = null;
        }

        public override string ToString()
        {
            string initStatus = IsInitialized ? "✓" : "✗";
            string valueStr = Value?.ToString() ?? "null";
            return $"{Name,-15} {Type,-12} {DataType,-10} {Scope,-8} {initStatus,-4} {Line}:{Column,-3} {valueStr}";
        }
    }

    // Symbol Table class
    public class SymbolTable
    {
        private Dictionary<string, SymbolTableEntry> symbols;
        private Dictionary<string, HashSet<string>> scopeSymbols;
        private string currentScope;

        public SymbolTable()
        {
            symbols = new Dictionary<string, SymbolTableEntry>();
            scopeSymbols = new Dictionary<string, HashSet<string>>();
            currentScope = "global";
            scopeSymbols[currentScope] = new HashSet<string>();
        }

        public void EnterScope(string scopeName)
        {
            currentScope = scopeName;
            if (!scopeSymbols.ContainsKey(scopeName))
            {
                scopeSymbols[scopeName] = new HashSet<string>();
            }
        }

        public void ExitScope()
        {
            currentScope = "global"; // Simplified - in real compiler, use scope stack
        }

        public bool Insert(string name, TokenType type, string dataType, int line, int column)
        {
            string key = $"{currentScope}.{name}";

            if (symbols.ContainsKey(key))
            {
                return false; // Symbol already exists in current scope
            }

            symbols[key] = new SymbolTableEntry(name, type, dataType, line, column, currentScope);
            scopeSymbols[currentScope].Add(name);
            return true;
        }

        public SymbolTableEntry Lookup(string name)
        {
            // First check current scope
            string key = $"{currentScope}.{name}";
            if (symbols.ContainsKey(key))
            {
                return symbols[key];
            }

            // Then check global scope
            key = $"global.{name}";
            if (symbols.ContainsKey(key))
            {
                return symbols[key];
            }

            return null;
        }

        public void UpdateValue(string name, object value)
        {
            var entry = Lookup(name);
            if (entry != null)
            {
                entry.Value = value;
                entry.IsInitialized = true;
            }
        }

        public void DisplaySymbolTable()
        {
            Console.WriteLine("\n=== SYMBOL TABLE ===");
            Console.WriteLine($"{"Name",-15} {"Type",-12} {"DataType",-10} {"Scope",-8} {"Init",-4} {"Pos",-6} {"Value"}");
            Console.WriteLine(new string('-', 70));

            var sortedSymbols = symbols.Values.OrderBy(s => s.Scope).ThenBy(s => s.Name);
            foreach (var symbol in sortedSymbols)
            {
                Console.WriteLine(symbol.ToString());
            }
        }

        public Dictionary<string, SymbolTableEntry> GetAllSymbols()
        {
            return new Dictionary<string, SymbolTableEntry>(symbols);
        }
    }

    // Lexical Analyzer class
    public class LexicalAnalyzer
    {
        private string sourceCode;
        private int position;
        private int line;
        private int column;
        private SymbolTable symbolTable;

        // C-like language keywords
        private readonly HashSet<string> keywords = new HashSet<string>
        {
            "int", "float", "double", "char", "void", "bool",
            "if", "else", "while", "for", "do", "switch", "case", "default",
            "break", "continue", "return", "goto",
            "struct", "union", "enum", "typedef",
            "const", "static", "extern", "auto", "register",
            "sizeof", "true", "false", "null"
        };

        // Operators
        private readonly HashSet<string> operators = new HashSet<string>
        {
            "+", "-", "*", "/", "%", "++", "--",
            "==", "!=", "<", ">", "<=", ">=",
            "&&", "||", "!", "&", "|", "^", "~", "<<", ">>",
            "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>="
        };

        // Delimiters
        private readonly HashSet<char> delimiters = new HashSet<char>
        {
            '(', ')', '{', '}', '[', ']', ';', ',', '.', ':', '?'
        };

        public LexicalAnalyzer(string sourceCode, SymbolTable symbolTable)
        {
            this.sourceCode = sourceCode;
            this.symbolTable = symbolTable;
            position = 0;
            line = 1;
            column = 1;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (position < sourceCode.Length)
            {
                Token token = GetNextToken();
                if (token != null)
                {
                    tokens.Add(token);

                    // Integrate with symbol table
                    if (token.Type == TokenType.IDENTIFIER)
                    {
                        ProcessIdentifier(token);
                    }
                }
            }

            tokens.Add(new Token(TokenType.EOF, "", line, column));
            return tokens;
        }

        private void ProcessIdentifier(Token token)
        {
            // Check if identifier already exists in symbol table
            var existingSymbol = symbolTable.Lookup(token.Lexeme);

            if (existingSymbol == null)
            {
                // New identifier - add to symbol table with unknown type initially
                symbolTable.Insert(token.Lexeme, TokenType.IDENTIFIER, "unknown", token.Line, token.Column);
            }
        }

        private Token GetNextToken()
        {
            SkipWhitespace();

            if (position >= sourceCode.Length)
                return null;

            int startLine = line;
            int startColumn = column;
            char currentChar = sourceCode[position];

            // Single-line comment
            if (currentChar == '/' && Peek() == '/')
            {
                return ReadSingleLineComment(startLine, startColumn);
            }

            // Multi-line comment
            if (currentChar == '/' && Peek() == '*')
            {
                return ReadMultiLineComment(startLine, startColumn);
            }

            // String literal
            if (currentChar == '"')
            {
                return ReadStringLiteral(startLine, startColumn);
            }

            // Character literal
            if (currentChar == '\'')
            {
                return ReadCharLiteral(startLine, startColumn);
            }

            // Number
            if (char.IsDigit(currentChar))
            {
                return ReadNumber(startLine, startColumn);
            }

            // Identifier or keyword
            if (char.IsLetter(currentChar) || currentChar == '_')
            {
                return ReadIdentifierOrKeyword(startLine, startColumn);
            }

            // Assignment operator
            if (currentChar == '=')
            {
                if (Peek() == '=')
                {
                    Advance();
                    Advance();
                    return new Token(TokenType.OPERATOR, "==", startLine, startColumn);
                }
                else
                {
                    Advance();
                    return new Token(TokenType.ASSIGNMENT, "=", startLine, startColumn);
                }
            }

            // Multi-character operators
            string multiCharOp = ReadMultiCharOperator();
            if (multiCharOp != null)
            {
                return new Token(TokenType.OPERATOR, multiCharOp, startLine, startColumn);
            }

            // Single character operators
            if (operators.Contains(currentChar.ToString()))
            {
                char op = currentChar;
                Advance();
                return new Token(TokenType.OPERATOR, op.ToString(), startLine, startColumn);
            }

            // Delimiters
            if (delimiters.Contains(currentChar))
            {
                char delimiter = currentChar;
                Advance();
                return new Token(TokenType.DELIMITER, delimiter.ToString(), startLine, startColumn);
            }

            // Newline
            if (currentChar == '\n')
            {
                Advance();
                return new Token(TokenType.NEWLINE, "\\n", startLine, startColumn);
            }

            // Error - unknown character
            char errorChar = currentChar;
            Advance();
            return new Token(TokenType.ERROR, errorChar.ToString(), startLine, startColumn);
        }

        private Token ReadSingleLineComment(int startLine, int startColumn)
        {
            StringBuilder comment = new StringBuilder();

            while (position < sourceCode.Length && sourceCode[position] != '\n')
            {
                comment.Append(sourceCode[position]);
                Advance();
            }

            return new Token(TokenType.COMMENT, comment.ToString(), startLine, startColumn);
        }

        private Token ReadMultiLineComment(int startLine, int startColumn)
        {
            StringBuilder comment = new StringBuilder();
            Advance(); // skip '/'
            Advance(); // skip '*'
            comment.Append("/*");

            while (position < sourceCode.Length - 1)
            {
                if (sourceCode[position] == '*' && sourceCode[position + 1] == '/')
                {
                    comment.Append("*/");
                    Advance();
                    Advance();
                    break;
                }
                comment.Append(sourceCode[position]);
                Advance();
            }

            return new Token(TokenType.COMMENT, comment.ToString(), startLine, startColumn);
        }

        private Token ReadStringLiteral(int startLine, int startColumn)
        {
            StringBuilder str = new StringBuilder();
            Advance(); // skip opening quote

            while (position < sourceCode.Length && sourceCode[position] != '"')
            {
                if (sourceCode[position] == '\\' && position + 1 < sourceCode.Length)
                {
                    Advance();
                    char escapeChar = sourceCode[position];
                    switch (escapeChar)
                    {
                        case 'n': str.Append('\n'); break;
                        case 't': str.Append('\t'); break;
                        case 'r': str.Append('\r'); break;
                        case '\\': str.Append('\\'); break;
                        case '"': str.Append('"'); break;
                        default: str.Append(escapeChar); break;
                    }
                }
                else
                {
                    str.Append(sourceCode[position]);
                }
                Advance();
            }

            if (position < sourceCode.Length)
                Advance(); // skip closing quote

            return new Token(TokenType.STRING, $"\"{str}\"", startLine, startColumn, str.ToString());
        }

        private Token ReadCharLiteral(int startLine, int startColumn)
        {
            Advance(); // skip opening quote
            char charValue = sourceCode[position];

            if (sourceCode[position] == '\\' && position + 1 < sourceCode.Length)
            {
                Advance();
                switch (sourceCode[position])
                {
                    case 'n': charValue = '\n'; break;
                    case 't': charValue = '\t'; break;
                    case 'r': charValue = '\r'; break;
                    case '\\': charValue = '\\'; break;
                    case '\'': charValue = '\''; break;
                    default: charValue = sourceCode[position]; break;
                }
            }

            Advance();
            if (position < sourceCode.Length && sourceCode[position] == '\'')
                Advance(); // skip closing quote

            return new Token(TokenType.CHAR, $"'{charValue}'", startLine, startColumn, charValue);
        }

        private Token ReadNumber(int startLine, int startColumn)
        {
            StringBuilder number = new StringBuilder();
            bool isFloat = false;

            while (position < sourceCode.Length && (char.IsDigit(sourceCode[position]) || sourceCode[position] == '.'))
            {
                if (sourceCode[position] == '.')
                {
                    if (isFloat) break; // Second dot, not part of this number
                    isFloat = true;
                }
                number.Append(sourceCode[position]);
                Advance();
            }

            string numberStr = number.ToString();
            TokenType type = isFloat ? TokenType.FLOAT : TokenType.INTEGER;
            object value = isFloat ? (object)double.Parse(numberStr) : int.Parse(numberStr);

            return new Token(type, numberStr, startLine, startColumn, value);
        }

        private Token ReadIdentifierOrKeyword(int startLine, int startColumn)
        {
            StringBuilder identifier = new StringBuilder();

            while (position < sourceCode.Length &&
                   (char.IsLetterOrDigit(sourceCode[position]) || sourceCode[position] == '_'))
            {
                identifier.Append(sourceCode[position]);
                Advance();
            }

            string identifierStr = identifier.ToString();
            TokenType type = keywords.Contains(identifierStr) ? TokenType.KEYWORD : TokenType.IDENTIFIER;

            return new Token(type, identifierStr, startLine, startColumn);
        }

        private string ReadMultiCharOperator()
        {
            if (position >= sourceCode.Length - 1)
                return null;

            string twoChar = sourceCode.Substring(position, 2);
            if (operators.Contains(twoChar))
            {
                Advance();
                Advance();
                return twoChar;
            }

            return null;
        }

        private void SkipWhitespace()
        {
            while (position < sourceCode.Length &&
                   (sourceCode[position] == ' ' || sourceCode[position] == '\t' || sourceCode[position] == '\r'))
            {
                Advance();
            }
        }

        private char Peek()
        {
            return position + 1 < sourceCode.Length ? sourceCode[position + 1] : '\0';
        }

        private void Advance()
        {
            if (position < sourceCode.Length)
            {
                if (sourceCode[position] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
                position++;
            }
        }
    }

    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Integration: Lexical Analyzer and Symbol Table (Phase-1)");
            Console.WriteLine(new string('=', 60));

            // Example 1: Simple C-like program
            Console.WriteLine("\nExample 1: Simple Variable Declarations");
            string code1 = @"
int x = 10;
float y = 3.14;
char ch = 'A';
int sum = x + 5;
";
            ProcessCode(code1);

            // Example 2: More complex program
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Example 2: Function with Variables");
            string code2 = @"
int factorial(int n) {
    int result = 1;
    for (int i = 1; i <= n; i++) {
        result = result * i;
    }
    return result;
}
";
            ProcessCode(code2);

            // Example 3: Program with different data types
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Example 3: Various Data Types");
            string code3 = @"
bool flag = true;
double pi = 3.14159;
char greeting[] = ""Hello World"";
int numbers[5] = {1, 2, 3, 4, 5};
float average = 0.0;
";
            ProcessCode(code3);

            // Interactive mode
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Interactive Mode");
            InteractiveMode();
        }

        static void ProcessCode(string sourceCode)
        {
            SymbolTable symbolTable = new SymbolTable();
            LexicalAnalyzer lexer = new LexicalAnalyzer(sourceCode, symbolTable);

            Console.WriteLine("\nSource Code:");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine(sourceCode.Trim());

            List<Token> tokens = lexer.Tokenize();

            Console.WriteLine("\nTokens Generated:");
            Console.WriteLine(new string('-', 40));
            foreach (var token in tokens.Where(t => t.Type != TokenType.EOF))
            {
                Console.WriteLine(token);
            }

            // Perform simple semantic analysis for variable declarations
            AnalyzeVariableDeclarations(tokens, symbolTable);

            symbolTable.DisplaySymbolTable();
        }

        static void AnalyzeVariableDeclarations(List<Token> tokens, SymbolTable symbolTable)
        {
            for (int i = 0; i < tokens.Count - 2; i++)
            {
                // Pattern: type identifier = value
                if (tokens[i].Type == TokenType.KEYWORD && IsDataType(tokens[i].Lexeme) &&
                    tokens[i + 1].Type == TokenType.IDENTIFIER)
                {
                    string dataType = tokens[i].Lexeme;
                    string varName = tokens[i + 1].Lexeme;

                    // Update symbol table entry with correct data type
                    var symbol = symbolTable.Lookup(varName);
                    if (symbol != null)
                    {
                        symbol.DataType = dataType;

                        // Check for initialization
                        if (i + 2 < tokens.Count && tokens[i + 2].Type == TokenType.ASSIGNMENT)
                        {
                            if (i + 3 < tokens.Count)
                            {
                                var valueToken = tokens[i + 3];
                                symbolTable.UpdateValue(varName, valueToken.Value);
                            }
                        }
                    }
                }
            }
        }

        static bool IsDataType(string keyword)
        {
            return new[] { "int", "float", "double", "char", "bool", "void" }.Contains(keyword);
        }

        static void InteractiveMode()
        {
            Console.WriteLine("Enter C-like code (type 'END' on a new line to finish):");

            StringBuilder codeBuilder = new StringBuilder();
            string line;

            while ((line = Console.ReadLine()) != "END")
            {
                codeBuilder.AppendLine(line);
            }

            string code = codeBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(code))
            {
                Console.WriteLine("\nProcessing your code...");
                ProcessCode(code);
            }
        }
    }
}