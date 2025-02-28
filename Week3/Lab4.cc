using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LexicalAnalyzer
{
    // Define token types
    public enum TokenType
    {
        IDENTIFIER,
        NUMBER,
        OPERATOR,
        KEYWORD,
        STRING_LITERAL,
        COMMENT,
        PARENTHESIS,
        BRACKET,
        BRACE,
        SEMICOLON,
        COMMA,
        EOF,
        UNKNOWN
    }

    // Token class representing lexical tokens
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"Token({Type}, '{Value}', line={Line}, column={Column})";
        }
    }

    // Lexical Analyzer using two-buffer approach
    public class LexicalAnalyzer
    {
        private const int BUFFER_SIZE = 4096;  // Size of each buffer
        private const char EOF_MARKER = '\0';  // Using null character as EOF marker instead of -1

        private readonly string[] _keywords = { "if", "else", "while", "for", "return", "int", "string", "bool", "class", "void" };
        private readonly char[] _operators = { '+', '-', '*', '/', '%', '=', '>', '<', '!', '&', '|' };

        private readonly char[] _buffer1;      // First buffer
        private readonly char[] _buffer2;      // Second buffer
        private readonly StreamReader _reader;  // Input stream

        private int _currentBufferIndex;       // Current buffer (0 or 1)
        private int _bufferPosition;           // Position in current buffer
        private int _line;                     // Current line number
        private int _column;                   // Current column number
        private bool _endOfFile;               // Flag to indicate EOF
        private bool _eof1;                    // Flag to indicate if buffer1 has EOF
        private bool _eof2;                    // Flag to indicate if buffer2 has EOF

        public LexicalAnalyzer(string filePath)
        {
            _buffer1 = new char[BUFFER_SIZE + 1];  // +1 for sentinel character
            _buffer2 = new char[BUFFER_SIZE + 1];  // +1 for sentinel character
            _reader = new StreamReader(filePath);
            _currentBufferIndex = 0;
            _bufferPosition = 0;
            _line = 1;
            _column = 1;
            _endOfFile = false;
            _eof1 = false;
            _eof2 = false;

            // Initialize both buffers
            LoadBuffer(0);
            LoadBuffer(1);
        }

        // Load data into the specified buffer (0 for buffer1, 1 for buffer2)
        private void LoadBuffer(int bufferIndex)
        {
            char[] buffer = bufferIndex == 0 ? _buffer1 : _buffer2;

            // Read characters into buffer
            int charsRead = _reader.Read(buffer, 0, BUFFER_SIZE);

            // Set sentinel at the end of valid data
            if (charsRead < BUFFER_SIZE)
            {
                buffer[charsRead] = EOF_MARKER;

                if (bufferIndex == 0)
                    _eof1 = true;
                else
                    _eof2 = true;

                if (charsRead == 0)
                {
                    _endOfFile = true;
                }
            }
        }

        // Get the current character
        private char GetCurrentChar()
        {
            if (_endOfFile)
            {
                return EOF_MARKER;
            }

            char[] currentBuffer = _currentBufferIndex == 0 ? _buffer1 : _buffer2;
            return currentBuffer[_bufferPosition];
        }

        // Check if currently at EOF position
        private bool IsAtEof()
        {
            if (_endOfFile)
                return true;

            // Check if we're at the EOF marker in the current buffer
            if ((_currentBufferIndex == 0 && _eof1 && _bufferPosition >= Array.IndexOf(_buffer1, EOF_MARKER)) ||
                (_currentBufferIndex == 1 && _eof2 && _bufferPosition >= Array.IndexOf(_buffer2, EOF_MARKER)))
            {
                return true;
            }

            return false;
        }

        // Advance to the next character
        private void Advance()
        {
            if (IsAtEof())
            {
                _endOfFile = true;
                return;
            }

            _column++;
            _bufferPosition++;

            // If we reached the end of the current buffer, switch to the other buffer
            if (_bufferPosition == BUFFER_SIZE)
            {
                _currentBufferIndex = 1 - _currentBufferIndex;  // Toggle between 0 and 1
                _bufferPosition = 0;

                // Reload the buffer we just finished
                LoadBuffer(1 - _currentBufferIndex);
            }

            // Handle newline characters
            if (GetCurrentChar() == '\n')
            {
                _line++;
                _column = 1;
            }
        }

        // Skip whitespace
        private void SkipWhitespace()
        {
            while (!IsAtEof() && char.IsWhiteSpace(GetCurrentChar()))
            {
                Advance();
            }
        }

        // Get the next token from the input
        public Token GetNextToken()
        {
            SkipWhitespace();

            if (IsAtEof())
            {
                return new Token(TokenType.EOF, "EOF", _line, _column);
            }

            char currentChar = GetCurrentChar();
            int startLine = _line;
            int startColumn = _column;

            // Identifier or keyword
            if (char.IsLetter(currentChar) || currentChar == '_')
            {
                return ScanIdentifierOrKeyword(startLine, startColumn);
            }

            // Number
            if (char.IsDigit(currentChar))
            {
                return ScanNumber(startLine, startColumn);
            }

            // String literal
            if (currentChar == '"')
            {
                return ScanStringLiteral(startLine, startColumn);
            }

            // Comment
            if (currentChar == '/' && (PeekNext() == '/' || PeekNext() == '*'))
            {
                return ScanComment(startLine, startColumn);
            }

            // Operators
            if (Array.IndexOf(_operators, currentChar) >= 0)
            {
                return ScanOperator(startLine, startColumn);
            }

            // Parentheses
            if (currentChar == '(' || currentChar == ')')
            {
                string value = currentChar.ToString();
                Advance();
                return new Token(TokenType.PARENTHESIS, value, startLine, startColumn);
            }

            // Brackets
            if (currentChar == '[' || currentChar == ']')
            {
                string value = currentChar.ToString();
                Advance();
                return new Token(TokenType.BRACKET, value, startLine, startColumn);
            }

            // Braces
            if (currentChar == '{' || currentChar == '}')
            {
                string value = currentChar.ToString();
                Advance();
                return new Token(TokenType.BRACE, value, startLine, startColumn);
            }

            // Semicolon
            if (currentChar == ';')
            {
                Advance();
                return new Token(TokenType.SEMICOLON, ";", startLine, startColumn);
            }

            // Comma
            if (currentChar == ',')
            {
                Advance();
                return new Token(TokenType.COMMA, ",", startLine, startColumn);
            }

            // Unknown
            string unknown = currentChar.ToString();
            Advance();
            return new Token(TokenType.UNKNOWN, unknown, startLine, startColumn);
        }

        // Peek at the next character
        private char PeekNext()
        {
            if (IsAtEof())
                return EOF_MARKER;

            int nextPosition = _bufferPosition + 1;
            int nextBufferIndex = _currentBufferIndex;

            // If next position is beyond current buffer, look at start of other buffer
            if (nextPosition == BUFFER_SIZE)
            {
                nextPosition = 0;
                nextBufferIndex = 1 - nextBufferIndex;
            }

            char[] buffer = nextBufferIndex == 0 ? _buffer1 : _buffer2;
            return buffer[nextPosition];
        }

        // Scan an identifier or keyword
        private Token ScanIdentifierOrKeyword(int startLine, int startColumn)
        {
            StringBuilder value = new StringBuilder();

            while (!IsAtEof() && (char.IsLetterOrDigit(GetCurrentChar()) || GetCurrentChar() == '_'))
            {
                value.Append(GetCurrentChar());
                Advance();
            }

            string lexeme = value.ToString();

            // Check if it's a keyword
            if (Array.IndexOf(_keywords, lexeme) >= 0)
            {
                return new Token(TokenType.KEYWORD, lexeme, startLine, startColumn);
            }

            return new Token(TokenType.IDENTIFIER, lexeme, startLine, startColumn);
        }

        // Scan a number
        private Token ScanNumber(int startLine, int startColumn)
        {
            StringBuilder value = new StringBuilder();
            bool hasDecimalPoint = false;

            while (!IsAtEof() && (char.IsDigit(GetCurrentChar()) ||
                   (GetCurrentChar() == '.' && !hasDecimalPoint)))
            {
                if (GetCurrentChar() == '.')
                {
                    hasDecimalPoint = true;
                }

                value.Append(GetCurrentChar());
                Advance();
            }

            return new Token(TokenType.NUMBER, value.ToString(), startLine, startColumn);
        }

        // Scan a string literal
        private Token ScanStringLiteral(int startLine, int startColumn)
        {
            StringBuilder value = new StringBuilder();
            value.Append(GetCurrentChar()); // Include opening quote
            Advance();

            while (!IsAtEof() && GetCurrentChar() != '"')
            {
                // Handle escape sequences
                if (GetCurrentChar() == '\\' && PeekNext() == '"')
                {
                    value.Append(GetCurrentChar());
                    Advance();
                }

                value.Append(GetCurrentChar());
                Advance();
            }

            if (!IsAtEof() && GetCurrentChar() == '"')
            {
                value.Append(GetCurrentChar()); // Include closing quote
                Advance();
            }

            return new Token(TokenType.STRING_LITERAL, value.ToString(), startLine, startColumn);
        }

        // Scan a comment
        private Token ScanComment(int startLine, int startColumn)
        {
            StringBuilder value = new StringBuilder();
            value.Append(GetCurrentChar()); // Include first slash
            Advance();

            // Single-line comment
            if (!IsAtEof() && GetCurrentChar() == '/')
            {
                value.Append(GetCurrentChar());
                Advance();

                while (!IsAtEof() && GetCurrentChar() != '\n')
                {
                    value.Append(GetCurrentChar());
                    Advance();
                }

                return new Token(TokenType.COMMENT, value.ToString(), startLine, startColumn);
            }

            // Multi-line comment
            if (!IsAtEof() && GetCurrentChar() == '*')
            {
                value.Append(GetCurrentChar());
                Advance();

                while (!IsAtEof() && !(GetCurrentChar() == '*' && PeekNext() == '/'))
                {
                    value.Append(GetCurrentChar());
                    Advance();
                }

                if (!IsAtEof() && GetCurrentChar() == '*')
                {
                    value.Append(GetCurrentChar());
                    Advance();

                    if (!IsAtEof() && GetCurrentChar() == '/')
                    {
                        value.Append(GetCurrentChar());
                        Advance();
                    }
                }

                return new Token(TokenType.COMMENT, value.ToString(), startLine, startColumn);
            }

            // Should not reach here, but just in case
            return new Token(TokenType.OPERATOR, "/", startLine, startColumn);
        }

        // Scan an operator
        private Token ScanOperator(int startLine, int startColumn)
        {
            StringBuilder value = new StringBuilder();
            value.Append(GetCurrentChar());
            char firstChar = GetCurrentChar();
            Advance();

            // Handle compound operators (==, !=, >=, <=, &&, ||)
            if (!IsAtEof() && ((firstChar == '=' && GetCurrentChar() == '=') ||
                (firstChar == '!' && GetCurrentChar() == '=') ||
                (firstChar == '>' && GetCurrentChar() == '=') ||
                (firstChar == '<' && GetCurrentChar() == '=') ||
                (firstChar == '&' && GetCurrentChar() == '&') ||
                (firstChar == '|' && GetCurrentChar() == '|')))
            {
                value.Append(GetCurrentChar());
                Advance();
            }

            return new Token(TokenType.OPERATOR, value.ToString(), startLine, startColumn);
        }

        // Close the analyzer and release resources
        public void Close()
        {
            _reader.Close();
        }
    }

    // Usage example
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get the directory where the executable is located
                

                // Build the path to code.txt in the same directory
                string filePath = "week3\\code.txt";

                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: Could not find code.txt");
                    Console.WriteLine("Please make sure code.txt is in the same directory as the program.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Processing file: {filePath}");
                LexicalAnalyzer analyzer = new LexicalAnalyzer(filePath);
                Token token;
                int tokenCount = 0;

                // Process all tokens until EOF
                do
                {
                    token = analyzer.GetNextToken();
                    Console.WriteLine(token);
                    tokenCount++;
                } while (token.Type != TokenType.EOF);

                analyzer.Close();

                Console.WriteLine($"Analysis complete. Found {tokenCount} tokens.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
