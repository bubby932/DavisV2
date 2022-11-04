namespace Davis.Parsing
{
	public class Scanner
	{
		public bool Success = true;

		private static readonly Dictionary<string, TokenType> keywords = new()
		{
			{ "struct", TokenType.Struct },
			{ "else", TokenType.Else },
			{ "false", TokenType.False },
			{ "for", TokenType.For },
			{ "function", TokenType.Function },
			{ "if", TokenType.If },
			{ "return", TokenType.Return },
			{ "true", TokenType.True },
			{ "var", TokenType.Var },
			{ "while", TokenType.While },
			{ "packed", TokenType.Packed }
		};

		private readonly string source;
		private readonly List<Token> tokens = new();

		private int start = 0;
		private int current = 0;
		private int line = 1;

		public Scanner(string source)
		{
			this.source = source;
		}

		private bool IsAtEnd() => current >= source.Length;
		private char Advance()
		{
			return source[current++];
		}
		private void AddToken(TokenType type) => AddToken(type, null);
		private void AddToken(TokenType type, object literal)
		{
			string text = source[start..current];
			tokens.Add(new Token(type, text, literal, line));
		}

		public List<Token> ScanTokens()
		{
			while(!IsAtEnd())
			{
				start = current;
				ScanToken();
			}

			tokens.Add(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}

		private void ScanToken()
		{
			char c = Advance();
			switch(c)
			{
				case '(': AddToken(TokenType.LeftParen); break;
				case ')': AddToken(TokenType.RightParen); break;
				case '{': AddToken(TokenType.LeftBracket); break;
				case '}': AddToken(TokenType.RightBracket); break;
				case ',': AddToken(TokenType.Comma); break;
				case '.': AddToken(TokenType.Period); break;
				case '+': AddToken(TokenType.Plus); break;
				case ';': AddToken(TokenType.Semicolon); break;
				case '*': AddToken(TokenType.Star); break;

				case '!':
					AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
					break;
				case '=':
					AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
					break;
				case '<':
					AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
					break;
				case '>':
					AddToken(Match('>') ? TokenType.GreaterEqual : TokenType.Greater);
					break;
				case '&':
					AddToken(Match('&') ? TokenType.BooleanAnd : TokenType.BitwiseAnd);
					break;
				case '|':
					AddToken(Match('|') ? TokenType.BooleanOr : TokenType.BitwiseOr);
					break;
				case '-':
					AddToken(Match('>') ? TokenType.IndirectionArrow : TokenType.Minus);
					break;
				case '/':
					if(Match('/'))
					{
						while (Peek() != '\n' && !IsAtEnd()) Advance();
					} else
					{
						AddToken(TokenType.Slash);
					}
					break;

				case ' ':	// Fallthrough
				case '\r':
				case '\t':
					break;

				case '\n':
					line++;
					break;

				case '"': HandleString(); break;

				default:
					if(IsDigit(c))
					{
						HandleNumber();
					} else if (IsAlpha(c))
					{
						HandleIdentifier();
					} else
					{
						Console.WriteLine($"[ Syntax Error] Unexpected character {c} at {line}.");
						Success = false;
					}
					break;
			}
		}

		private void HandleIdentifier()
		{
			while (IsAlphaNumeric(Peek())) Advance();

			string text = source[start..current];
			TokenType type = keywords.GetValueOrDefault(text, TokenType.Identifier);

			AddToken(type, text);
		}

		private void HandleNumber()
		{
			while (IsDigit(Peek())) Advance();

			if(Peek() == '.' && IsDigit(PeekNext())) {
				Advance();

				while (IsDigit(Peek())) Advance();
			}

			AddToken(TokenType.NumericLiteral,
				double.Parse(source[start..current]));
		}

		private void HandleString()
		{
			while(Peek() != '"' && !IsAtEnd())
			{
				if (Peek() == '\n') line++;
				Advance();
			}

			if(IsAtEnd())
			{
				Success = false;
				Console.WriteLine($"[ Syntax Error ] Unterminated string at line {line}");
				return;
			}

			Advance();

			string value = source.Substring(start + 1, (current - start) - 1);
			AddToken(TokenType.StringLiteral, value);
		}
		
		private bool Match(char expected)
		{
			if (IsAtEnd()) return false;
			if (source[current] != expected) return false;

			current++;
			return true;
		}

		private char Peek()
		{
			if (IsAtEnd()) return '\0';
			return source[current];
		}

		private char PeekNext()
		{
			if (current + 1 >= source.Length) return '\0';
			return source[current + 1];
		}

		private static bool IsAlpha(char c) =>
			(c >= 'a' && c <= 'z') ||
			(c >= 'A' && c <= 'Z') ||
			 c == '_';

		private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

		private static bool IsDigit(char c) => c >= '0' && c <= '9';
	}
}