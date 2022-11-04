using System.Diagnostics;
using Davis.Compilation;
using Davis.Parsing;
using Davis.Preprocessor;

namespace Davis
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if(args.Length < 2)
			{
				Usage();
				return;
			}

			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"[ Critical ] No file found at {Path.GetFullPath(args[0])}");
				return;
			}

			string initialSource;
			try
			{
				initialSource = File.ReadAllText(args[0]);
			}
			catch
			{
				Console.WriteLine($"[ Critical ] Failed to read {args[0]}, do you have permission to read that file?");
				return;
			}

			DavisPreprocessor preprocessor = new DavisPreprocessor(initialSource);

			string source = preprocessor.Preprocess();

			Scanner scanner = new(source);
			Token[] tokens = scanner.ScanTokens().ToArray();

			if (!scanner.Success) return;

			Compiler compiler = new(tokens);

			string assembly = compiler.Compile();

			if (!compiler.Success) return;

			File.WriteAllText($"{args[1]}.asm", assembly);

			Process proc = Process.Start("nasm", $"{args[1]}.asm");

			proc.WaitForExit();
			Console.Write(proc.ExitCode);
		}

		static void Usage()
		{
			Console.WriteLine("USAGE:");
			Console.WriteLine("  davis <input path> <output path> [addtl. args]");
		}
	}
}