namespace Davis.Parsing {
	public class Token
	{
		public readonly TokenType type;
		public readonly string lexeme;
		public readonly object literal;
		public readonly int line;

		public Token(
			TokenType type,
			string lexeme,
			object literal,
			int line
			)
		{
			this.type = type;
			this.lexeme = lexeme;
			this.literal = literal;
			this.line = line;
		}

		public static implicit operator TokenType(Token t) => t.type;
		public static implicit operator string(Token t) => (string)t.literal;

		public override string ToString()
		{
			return $"{type} {lexeme} {literal}";
		}
	}

	public enum TokenType
	{
		// Individual Characters
		LeftParen, RightParen, LeftBracket, RightBracket,
		Comma, Period, Plus, Semicolon, Slash, Star,

		// Potentially multi-character tokens
		Bang, BangEqual,
		Equal, EqualEqual,
		Greater, GreaterEqual,
		Less, LessEqual,
		BitwiseAnd, BooleanAnd,
		BitwiseOr, BooleanOr,
		Minus, IndirectionArrow,

		// Literals
		Identifier, StringLiteral, NumericLiteral,

		// Keywords
		Struct, Else, False, Function, For, If,
		Return, True, Var, While, Packed, Pretend, Is,
		EntryPoint,

		// no
		EOF
	}
}