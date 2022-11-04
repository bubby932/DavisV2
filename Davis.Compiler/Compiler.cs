namespace Davis.Compilation
{
	using System.Text;
	using Davis.Parsing;
	public class Compiler
	{
		public bool Success = false;

		private readonly Token[] _tokens;

		private int _current = 0;
		private CodeGenState _state;


		public Compiler(Token[] tokens)
		{
			_tokens = tokens;
			_state = new CodeGenState();
		}

		public string Compile()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("bits 16 ; 16 Bit Real Mode\n");

			while (!IsAtEnd())
			{

			}
		}

		private void ExpectContext(CodeGenContext ctx) => ExpectContext(ctx, $"Invalid context for token, expected context {ctx}");
		private void ExpectContext(CodeGenContext ctx, string message)
		{
			if (_state != ctx)
			{
				throw new InvalidContextException(message);
			}
		}
		
		private void CompileError(string message)
		{
			Success = false;
			Console.WriteLine($"[ Compiler Error ] {message} at line {_tokens[_current].line}");
		}

		private bool IsAtEnd() => _current >= _tokens.Length;
		private Token Advance()
		{
			return _tokens[_current++];
		}
		private Token Match(TokenType expected)
		{
			if (IsAtEnd()) return null;
			if (_tokens[_current].type != expected) return null;

			_current++;
			return _tokens[_current];
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