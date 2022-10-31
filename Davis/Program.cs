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
				Console.WriteLine($"[ Critical ] No file found at {args[0]}");
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

			Console.WriteLine(source);
		}

		static void Usage()
		{
			Console.WriteLine("USAGE:");
			Console.WriteLine("  davis <input path> <output path> [addtl. args]");
		}
	}
}