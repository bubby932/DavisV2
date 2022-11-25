using Davis.StandardLibrary;

namespace Davis.Preprocessor
{
	public class DavisPreprocessor
	{
		public const int MAX_IMPORT_DEPTH = 50;

		private readonly string _InitialSource;

		private string _FinalSource;

		public List<string> PreprocessorDefines;
		public DavisPreprocessor(string Source)
		{
			_InitialSource = Source;
			_FinalSource = Source;
			PreprocessorDefines = new List<string>();
		}

		public string Preprocess()
		{
			PreprocessorFirstpass();
			PreprocessorSecondPass();

			return string.Join('\n', _FinalSource.Split('\n').Where(x => !x.StartsWith('#')));
		}

		/// <summary>
		/// Handles #with statements.
		/// </summary>
		private void PreprocessorFirstpass()
		{
			_FinalSource = HandleImports(_InitialSource, 0);
		}

		private string HandleImports(string file, int current_depth)
		{
			current_depth++;

			if (current_depth > MAX_IMPORT_DEPTH) throw new StackOverflowException("Recursive imports or otherwise too deep!");

			string[] lines = file.Split('\n');

			string final = "";

			for (int i = 0; i < lines.Length; i++)
			{
				string[] args = lines[i].TrimStart().Split(' ');

				switch(args[0].TrimEnd())
				{
					case "#with":
						{
							if (args.Length < 2) throw new PreprocessorException($"Too few arguments in #with statement at line {i}.");

							if (args[1].StartsWith('$'))
							{
								if (args.Length > 2) throw new PreprocessorException($"Too many arguments in #with statement at line {i}.");

								final += HandleImports(StdLib.BuiltinLibs[args[1].TrimEnd()], current_depth) + '\n';
							}
							else
							{
								string path = string.Join("", args[1..args.Length]).TrimEnd();
								if (!File.Exists(path)) throw new PreprocessorException($"Failed to locate file '{path}'\n unwrapped: {Path.GetFullPath(path)}");

								string src = File.ReadAllText(path);
								final += HandleImports(src, current_depth) + '\n';
							}
							break;
						}
					default:
						{
							final += lines[i] + '\n';
							break;
						}
				}
			}

			return final;
		}

		/// <summary>
		/// Handles defines & ifs.
		/// </summary>
		private void PreprocessorSecondPass()
		{
			int if_layer = 0;

			List<string> lines = _FinalSource.Split('\n').ToList();

			for (int i = 0; i < lines.Count; i++)
			{
				string line = lines[i];
				if (!line.TrimStart().StartsWith('#'))
				{
					continue;
				}

				string[] args = line.Split(' ');


				switch (args[0].Trim())
				{
					case "#ifundef":
						{
							if (args.Length > 2) throw new InvalidArgumentException($"Too many arguments to directive '#ifundef' at line {i + 1}");
							if (args.Length < 2) throw new InvalidArgumentException($"Too few arguments to directive '#ifundef' at line {i + 1}\nUSAGE:\n#ifundef {{SYMBOL}}");

							if (PreprocessorDefines.Contains(args[1]))
							{
								i = MatchUntilLayerReturns(lines, i+1);
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
								i = MatchUntilLayerReturns(lines, i + 1);
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
		private int MatchUntilLayerReturns(List<string> lines, int line)
		{
			int layer = 1;
			for (; line < lines.Count; line++)
			{
				string[] args = lines[line].TrimStart().Split(' ');
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
