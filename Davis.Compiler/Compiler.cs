namespace Davis.Compilation
{
	using System.Text;
	using Davis.Parsing;
	public class Compiler
	{
		public bool Success = true;

		private readonly Token[] _tokens;

		private int _current = 0;
		private readonly CodeGenState _state;


		public Compiler(Token[] tokens)
		{
			_tokens = tokens;
			_state = new CodeGenState();
		}

		public string Compile()
		{
			StringBuilder builder = new();

			builder.AppendLine("bits 16 ; 16 Bit Real Mode");

			while (!IsAtEnd())
			{
				Token next = Advance();
				if (next == TokenType.EOF)
				{
					ExpectContext(CodeGenContext.File, $"[ Unrecoverable Compile Error ] Unexpected end of file in {_state.Context}");
					break;
				}

				switch(next.type)
				{
					case TokenType.Function:
						{
							ExpectContext(CodeGenContext.File, "Cannot declare a function inside a struct or another function.");
							FunctionStub stub = ParseFunctionArguments(builder);

							_state.UpdateContext(CodeGenContext.Function, stub);

							builder.AppendLine($"; Davis function {stub.Name}");
							builder.AppendLine($"; Returns: {stub.ReturnType.Identifier}");
							builder.AppendLine( "; Arguments:");
							foreach(var item in stub.Arguments)
							{
								builder.AppendLine($";   - Name: {item.Item1}");
								builder.AppendLine($";     Type: {item.Item2.Identifier}");
								builder.AppendLine($";");
							}
							builder.AppendLine($"davis_function_{stub.Name}:");

							_ = Consume(TokenType.LeftBracket);

							_state.UpdateContext(CodeGenContext.Function, null);

							break;
						}
					case TokenType.Struct:
						{
							ExpectContext(CodeGenContext.File, "Cannot declare a structure type inside a function or another struct");
							throw new NotImplementedException("Structure types are not yet implemented.");
							break;
						}
					case TokenType.Identifier:
						{
							if(_state == CodeGenContext.File)
							{
								// Handle global variables
								throw new NotImplementedException();
							} else if(_state == CodeGenContext.Function)
							{
								if(_state.Types.ContainsKey((string)next.literal))
								{
									// Creating a variable;

								} else
								{
									bool global = _state.Globals.ContainsKey((string)next.literal);
									bool local = ((FunctionStub)_state.Scope).LocalNames.Contains((string)next.literal);
									// Assigning or calling a variable


									if (!global && !local) CompileError($"Assignment to undeclared variable {(string)next.literal}");

									DavisType variableType = local ?
										((FunctionStub)_state.Scope).Locals.Where(x => x.Item1 == (string)next.literal).First().Item2 :
										_state.Globals[(string)next.literal];

									Token op = Advance();

									if(op == TokenType.LeftParen)
									{

									}
										

									builder.AppendLine($"  ; Assignment to local variable '{next.literal}'");


								}
							} else
							{
								// Handle struct fields
								throw new NotImplementedException();
							}
							break;
						}
					default: throw new NotImplementedException($"[ Compiler Error ] Unimplemented token {next}!");
				}
			}

			return builder.ToString();
		}

		private FunctionStub ParseFunctionArguments(StringBuilder writer)
		{
			Token type_identifier = Consume(TokenType.Identifier);

			if (!_state.Types.ContainsKey((string)type_identifier.literal)) CompileError($"Use of undefined type {type_identifier.literal}");

			Token identifier = Consume(TokenType.Identifier);

			if (_state.Functions.ContainsKey((string)identifier.literal)) CompileError($"Redefinition of function {identifier.literal}");

			_ = Consume(TokenType.LeftParen);

			List<string> arg_names = new();
			List<(string, DavisType)> args = new();

			while(!IsAtEnd())
			{
				Token next = Advance();
				if (next == TokenType.EOF) throw new InvalidTokenException("Expected token ) or function argument, got EOF instead.");
				if (next == TokenType.RightParen) break;

				if (next != TokenType.Identifier) throw new InvalidTokenException($"Expected an identifier, got token {next} at line {next.line}");

				if (!_state.Types.ContainsKey((string)next.literal)) CompileError($"Use of undefined type `{next.literal}` in function parameter list.");

				Token param_name = Consume(TokenType.Identifier);

				if (arg_names.Contains((string)param_name.literal)) throw new InvalidTokenException($"Function parameter {param_name.literal} defined twice at line {param_name.literal}");

				args.Add(((string)param_name.literal, _state.Types[(string)next.literal]));
				arg_names.Add((string)next.literal);

				if(Peek() != TokenType.RightParen)
					_ = Consume(TokenType.Comma);
			}

			return new FunctionStub(args, _state.Types[(string)type_identifier.literal], (string)identifier.literal);
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

			return _tokens[_current++];
		}
		private Token Consume(TokenType expected)
		{
			if (IsAtEnd())
			{
				throw new InvalidTokenException($"Expected token {expected}, got EOF.");
			}
			if (_tokens[_current].type != expected) throw new InvalidTokenException($"Expected token {expected}, got token {_tokens[_current].type}");

			return _tokens[_current++];
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