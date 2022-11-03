namespace Davis.Compilation
{
	using Davis.Parsing;
	public class Compiler
	{
		private readonly Token[] _tokens;

		private int _current = 0;

		public Compiler(Token[] tokens)
		{
			_tokens = tokens;
		}



		private bool IsAtEnd() => _current >= _tokens.Length;
		private Token Advance()
		{
			return _tokens[_current++];
		}
		private bool Match(TokenType expected)
		{
			if (IsAtEnd()) return false;
			if (_tokens[_current].type != expected) return false;

			_current++;
			return true;
		}
		private Token Peek()
		{
			if (IsAtEnd()) return null;
			return _tokens[_current];
		}
		private Token PeekNext()
		{
			if (_current + 1 >= _tokens.Length) return null;
			return _tokens[_current + 1];
		}
	}
}