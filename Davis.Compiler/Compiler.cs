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
					case TokenType.EntryPoint:
						{
							Consume(TokenType.Function);
							FunctionStub f = Function(builder);
							if (f.ReturnType != DavisType.I8) throw new Exception("Entry point must return `i8`.");
							if (f.Arguments.Count > 0) throw new Exception("Entry point may not have any arguments.");

							_state.SetEntryPoint(f.Name);
							break;
						}
					case TokenType.Function:
						{
							Function(builder);
							break;
						}
					case TokenType.Struct:
						{
							ExpectContext(CodeGenContext.File);

							bool packed = Match(TokenType.Packed) != null;
							
							Token struct_name = Consume(TokenType.Identifier);
							if (_state.Types.ContainsKey(struct_name)) throw new Exception($"Structure type {struct_name.literal} is already defined.");

							Dictionary<string, StructField> fields = new();
							int size = 0;

							Consume(TokenType.LeftBracket);

							builder.AppendLine(packed ? $"; Struct `{struct_name.literal}` (Packed)" : $"; Struct `{struct_name.literal}`");
							builder.AppendLine("; Fields:");

							while(Match(TokenType.RightBracket) == null)
							{
								Token type = Consume(TokenType.Identifier);
								Token field_name = Consume(TokenType.Identifier);
								Consume(TokenType.Semicolon);

								DavisType t;
								if (!_state.Types.TryGetValue(type, out t))
								{
									throw new Exception($"Cannot use type {t} in structure when it is not defined.");
								}

								StructField field = new StructField(
									t,
									field_name,
									packed ? t.Size : Pad(t.Size),
									size
								);

								fields.Add(field_name, field);
								size += field.size;

								builder.AppendLine($"; - Name: {field_name.literal}");
								builder.AppendLine($";   Type: {t.Identifier}");
								builder.AppendLine($";   Size: {t.Size}");
								builder.AppendLine($";");
							}

							builder.AppendLine($"; Total Size: {size}");
							builder.AppendLine();

							DavisType @struct = new DavisType(struct_name, IntrinsicType.Structure, fields, size);
							_state.Types.Add(@struct.Identifier, @struct);

							break;
						}
					default: throw new NotImplementedException($"[ Compiler Error ] Unimplemented token {next}!");
				}
			}

			if (_state.EntryPoint == null) throw new Exception("No entry point defined.");

			builder.Insert(26, $"\ncall davis_function_{_state.EntryPoint}\n");

			return builder.ToString();
		}

		private FunctionStub Function(StringBuilder builder)
		{
			ExpectContext(CodeGenContext.File, "Cannot declare a function inside a struct or another function.");
			FunctionStub stub = ParseFunctionArguments();
			_state.EmittedReturnInstruction = false;

			_state.UpdateContext(CodeGenContext.Function, stub);

			builder.AppendLine($"; Davis function {stub.Name}");
			builder.AppendLine($"; Returns: {stub.ReturnType.Identifier}");
			builder.AppendLine("; Arguments:");
			foreach (var item in stub.Arguments)
			{
				builder.AppendLine($";   - Name: {item.Item1}");
				builder.AppendLine($";     Type: {item.Item2.Identifier}");
				builder.AppendLine($";");
			}
			builder.AppendLine($"davis_function_{stub.Name}:");

			_ = Consume(TokenType.LeftBracket);

			_state.UpdateContext(CodeGenContext.Function, null);
			_state.Locals = new();
			_state.LocalOffset = 0;


			while(Match(TokenType.RightBracket) == null)
			{
				Statement(builder);
			}

			builder.AppendLine($"  ; Stack cleanup.");
			builder.AppendLine($"  add sp, {-_state.LocalOffset}");
			if(!_state.EmittedReturnInstruction)
			{
				builder.AppendLine($"  ; Implicit return from function at end due to no 'return' keyword.");
				builder.AppendLine($"  ret");
			}

			builder.AppendLine($"; End of Davis function {stub.Name}");


			_state.UpdateContext(CodeGenContext.File, null);

			return stub;
		}

		private const int FIELD_ALIGNMENT = 2;
		private static int Pad(int size)
		{
			int hasRemainder = (size % FIELD_ALIGNMENT) > 0 ? 1 : 0;
			return ((size / FIELD_ALIGNMENT) + hasRemainder) * FIELD_ALIGNMENT;
		}

		private FunctionStub ParseFunctionArguments()
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

		private void Statement(StringBuilder _builder)
		{
			Token adv = Advance();
			switch(adv.type)
			{
				case TokenType.Pretend:
					{
						Token name = Consume(TokenType.Identifier);
						_ = Consume(TokenType.Is);
						Token type = Consume(TokenType.Identifier);
						_ = Consume(TokenType.Semicolon);

						if (!_state.Locals.ContainsKey((string)name.literal)) throw new Exception($"Cannot change type of undeclared variable {name.literal}");
						if (!_state.Types.ContainsKey((string)type.literal)) throw new Exception($"No type of name {type.literal} exists in this context.");

						_state.Locals[(string)name.literal].Item2.Type = _state.Types[(string)type.literal];

						_builder.AppendLine($"  ; pretend {name.literal} is {type.literal};");
						_builder.AppendLine($"  ; ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
						_builder.AppendLine($"  ;    Explicit unsafe cast occurred here.");
						break;
					}
				case TokenType.Bang:
					{
						// TODO implement formatting for variables etc
						Token asm = Consume(TokenType.StringLiteral);
						_builder.AppendLine("  ; Begin inline assembly.");
						_builder.AppendLine((string)asm.literal);
						_builder.AppendLine("  ; End inline assembly.");
						Consume(TokenType.Semicolon);
						break;
					}
				case TokenType.Identifier:
					{
						if(Peek().type == TokenType.Identifier)
						{
							// Variable
							DavisType T = _state.Types[(string)adv.literal];
							string name = (string)Advance().literal;

							if (Match(TokenType.Semicolon) != null)
							{
								_state.Locals.Add(name, (_state.LocalOffset, new Variable(name, T)));
								_state.LocalOffset -= Pad(T.Size);
								_builder.AppendLine($"  ; Variable `{T.Identifier} {name}` declared here, not assigned a value.");
								_builder.AppendLine($"  sub sp, {Pad(T.Size)}");
							}
							else throw new NotImplementedException();
						} else
						{
							throw new NotImplementedException(); // oh god expressions
						}
						break;
					}
				case TokenType.EOF:
					{
						throw new NotSupportedException();
					}
			}
		}

		private DavisType Expression(StringBuilder _builder)
		{

			throw new NotImplementedException();
		}

		private (string, DavisType) AssignmentTarget(Token root)
		{
			throw new NotImplementedException();
			StringBuilder b = new();

			if(Match(TokenType.Equal) == null)
			{

			} else
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