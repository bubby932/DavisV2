namespace Davis.Preprocessor
{
	public class DavisPreprocessor
	{
		private readonly string _InitialSource;
		private readonly string[] _Lines;

		private string _FinalSource;

		public List<string> PreprocessorDefines;
		public DavisPreprocessor(string Source)
		{
			_InitialSource = Source;
			_Lines = Source.Split('\n');
			_FinalSource = "";
			PreprocessorDefines = new List<string>();
		}

		public string Preprocess()
		{
			PreprocessorFirstpass();
			PreprocessorSecondPass();
			return _FinalSource;
		}

		private void PreprocessorFirstpass()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Handles defines & ifs.
		/// </summary>
		private void PreprocessorSecondPass()
		{
			int if_layer = 0;

			for (int i = 0; i < _Lines.Length; i++)
			{
				string line = _Lines[i];
				if (!line.TrimStart().StartsWith('#'))
				{
					_FinalSource += line + '\n';
					continue;
				}

				string[] args = line.Split(' ');


				switch (args[0].TrimEnd())
				{
					case "#ifundef":
						{
							if (args.Length > 2) throw new InvalidArgumentException($"Too many arguments to directive '#ifundef' at line {i + 1}");
							if (args.Length < 2) throw new InvalidArgumentException($"Too few arguments to directive '#ifundef' at line {i + 1}\nUSAGE:\n#ifundef {{SYMBOL}}");

							if (PreprocessorDefines.Contains(args[1]))
							{
								i = MatchUntilLayerReturns(i+1);
								break;
							} else
							{
								if_layer++;
							}

							break;
						}
					case "#ifdef":
						{
							if (args.Length > 2) throw new InvalidArgumentException($"Too many arguments to directive '#ifndef' at line {i + 1}");
							if (args.Length < 2) throw new InvalidArgumentException($"Too few arguments to directive '#ifdef' at line {i + 1}\nUSAGE:\n#ifdef {{SYMBOL}}");

							if (!PreprocessorDefines.Contains(args[1]))
							{
								i = MatchUntilLayerReturns(i + 1);
								break;
							}
							else
							{
								if_layer++;
							}

							break;
						}
					case "#define":
						{
							if (args.Length > 2) throw new InvalidArgumentException($"Too many arguments to directive '#define' at line {i + 1}");
							if (args.Length < 2) throw new InvalidArgumentException($"Too few arguments to directive '#define' at line {i + 1}\nUSAGE:\n#define {{SYMBOL}}");

							PreprocessorDefines.Add(args[1]);
							break;
						}
					case "#undefine":
						{
							if (args.Length > 2) throw new InvalidArgumentException($"Too many arguments to directive '#undefine' at line {i + 1}");
							if (args.Length < 2) throw new InvalidArgumentException($"Too few arguments to directive '#undefine' at line {i + 1}\nUSAGE:\n#undefine {{SYMBOL}}");

							PreprocessorDefines.Remove(args[1]);
							break;
						}
					case "#endif":
						{
							if_layer--;
							break;
						}
					default:
						throw new PreprocessorException($"Invalid preprocessor directive {args[0]} at line {i + 1}");
				}
			}
		}

		/// <summary>
		/// Matches until the #if stack returns to it's original layer.
		/// </summary>
		/// <param name="line">The line to begin matching at.</param>
		/// <returns>The line that the system stops at.</returns>
		/// <exception cref="PreprocessorException">Thrown if the layer becomes negative (More #endifs than #ifs)</exception>
		private int MatchUntilLayerReturns(int line)
		{
			int layer = 1;
			for (; line < _Lines.Length; line++)
			{
				string[] args = _Lines[line].TrimStart().Split(' ');
				switch (args[0].TrimEnd())
				{
					case "#endif":
						{
							layer--;

							if (layer == 0) return line + 1;
							break;
						}
					case "#ifdef":
						{
							layer++;
							break;
						}
					case "#ifundef":
						{
							layer++;
							break;
						}
					default: continue;
				}
			}

			throw new PreprocessorException($"No #endif statement in pair at line {line}");
		}
	}


	[Serializable]
	public class InvalidArgumentException : Exception
	{
		public InvalidArgumentException() { }
		public InvalidArgumentException(string message) : base(message) { }
		public InvalidArgumentException(string message, Exception inner) : base(message, inner) { }
		protected InvalidArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class PreprocessorException : Exception
	{
		public PreprocessorException() { }
		public PreprocessorException(string message) : base(message) { }
		public PreprocessorException(string message, Exception inner) : base(message, inner) { }
		protected PreprocessorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
