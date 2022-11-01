namespace Davis.Parsing
{
	public class Scanner
	{
		public bool Success = true;

		private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
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
			{ "while", TokenType.While }
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

		private bool isAtEnd() => current >= source.Length;
		private char advance()
		{
			return source[current++];
		}
		private void addToken(TokenType type) => addToken(type, null);
		private void addToken(TokenType type, object literal)
		{
			string text = source.Substring(start, current - start);
			tokens.Add(new Token(type, text, literal, line));
		}

		public List<Token> scanTokens()
		{
			while(!isAtEnd())
			{
				start = current;
				scanToken();
			}

			tokens.Add(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}

		private void scanToken()
		{
			char c = advance();
			switch(c)
			{
				case '(': addToken(TokenType.LeftParen); break;
				case ')': addToken(TokenType.RightParen); break;
				case '{': addToken(TokenType.LeftBracket); break;
				case '}': addToken(TokenType.RightBracket); break;
				case ',': addToken(TokenType.Comma); break;
				case '.': addToken(TokenType.Period); break;
				case '-': addToken(TokenType.Minus); break;
				case '+': addToken(TokenType.Plus); break;
				case ';': addToken(TokenType.Semicolon); break;
				case '*': addToken(TokenType.Star); break;

				case '!':
					addToken(match('=') ? TokenType.BangEqual : TokenType.Bang);
					break;
				case '=':
					addToken(match('=') ? TokenType.EqualEqual : TokenType.Equal);
					break;
				case '<':
					addToken(match('=') ? TokenType.LessEqual : TokenType.Less);
					break;
				case '>':
					addToken(match('>') ? TokenType.GreaterEqual : TokenType.Greater);
					break;
				case '&':
					addToken(match('&') ? TokenType.BooleanAnd : TokenType.BitwiseAnd);
					break;
				case '|':
					addToken(match('|') ? TokenType.BooleanOr : TokenType.BitwiseOr);
					break;
				case '/':
					if(match('/'))
					{
						while (peek() != '\n' && !isAtEnd()) advance();
					} else
					{
						addToken(TokenType.Slash);
					}
					break;

				case ' ':	// Fallthrough
				case '\r':
				case '\t':
					break;

				case '\n':
					line++;
					break;

				case '"': handle_string(); break;

				default:
					if(isDigit(c))
					{
						handle_number();
					} else
					{
						Console.WriteLine($"[ Syntax Error] Unexpected character {c} at {line}.");
						Success = false;
					}
					break;
			}
		}

		private void handle_identifier()
		{
			while (isAlphaNumeric(peek())) advance();

			string text = source.Substring(start, current - start);
			TokenType type = keywords.GetValueOrDefault(text, TokenType.Identifier);

			addToken(type);
		}

		private void handle_number()
		{
			while (isDigit(peek())) advance();

			if(peek() == '.' && isDigit(peekNext()) {
				advance();

				while (isDigit(peek())) advance();
			}

			addToken(TokenType.NumericLiteral,
				double.Parse(source.Substring(start, current - start)));
		}

		private void handle_string()
		{
			while(peek() != '"' && !isAtEnd())
			{
				if (peek() == '\n') line++;
				advance();
			}

			if(isAtEnd())
			{
				Success = false;
				Console.WriteLine($"[ Syntax Error ] Unterminated string at line {line}");
				return;
			}

			advance();

			string value = source.Substring(start + 1, (current - start) - 1);
			addToken(TokenType.StringLiteral, value);
		}
		
		private bool match(char expected)
		{
			if (isAtEnd()) return false;
			if (source[current] != expected) return false;

			current++;
			return true;
		}

		private char peek()
		{
			if (isAtEnd()) return '\0';
			return source[current];
		}

		private char peekNext()
		{
			if (current + 1 >= source.Length) return '\0';
			return source[current + 1];
		}

		private bool isAlpha(char c) =>
			(c >= 'a' && c <= 'z') ||
			(c >= 'A' && c <= 'Z') ||
			 c == '_';

		private bool isAlphaNumeric(char c) => isAlpha(c) || isDigit(c);

		private bool isDigit(char c) => c >= '0' && c <= '9';
	}
}