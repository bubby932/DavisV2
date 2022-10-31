namespace Davis.StandardLibrary
{
	public class StdLib
	{
		public static readonly Dictionary<string, string> BuiltinLibs = new Dictionary<string, string>()
		{
			{
				"$stdlib", 
				"#ifundef DAVIS_STDLIB\n\r" +
				"#define DAVIS_STDLIB\n\r" +
				"\n\r" +
				"#endif\n\r"
			}
		};
	}
}