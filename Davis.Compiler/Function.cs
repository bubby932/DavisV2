using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davis.Compilation
{
	internal class Function : IEquatable<Function>, IEquatable<string>
	{
		public string Name;
		public List<(string, DavisType)> Arguments;
		public string Assembly;

		public bool Equals(Function? other) => Name == other?.Name;
		public bool Equals(string? other) => Name.Equals(other);

		public static bool operator ==(Function a, string b) { return a.Equals(b); }
		public static bool operator !=(Function a, string b) { return !a.Equals(b); }

		public static bool operator ==(string a, Function b) { return b.Equals(a); }
		public static bool operator !=(string a, Function b) { return !b.Equals(a); }

		public static bool operator ==(Function a, Function b) { return a.Equals(b); }
		public static bool operator !=(Function a, Function b) { return !a.Equals(b); }

		public Function(string name, List<(string, DavisType)> arguments, string assembly)
		{
			Name = name;
			Arguments = arguments;
			Assembly = assembly;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Function);
		}

		public override int GetHashCode() => Name.GetHashCode();
	}

	internal class FunctionStub
	{
		public List<(string, DavisType)> Arguments;
		public DavisType ReturnType;
		public string Name;
		public List<(string, DavisType)> Locals = new();

		public FunctionStub(List<(string, DavisType)> arguments, DavisType returnType, string name)
		{
			Arguments = arguments;
			ReturnType = returnType;
			Name = name;
		}
	}

}
