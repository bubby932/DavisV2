namespace Davis.Compilation
{
	using Davis.Parsing;
	public class Compiler
	{
		private readonly List<Token> _Tokens;

		public Compiler(List<Token> tokens)
		{
			_Tokens = tokens;
		}
	}
}