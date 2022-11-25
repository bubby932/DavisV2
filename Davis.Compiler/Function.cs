using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davis.Compilation
{
	internal class StructField
	{
		public DavisType Type;
		public string Name;
		public int size;
		public int offset;

		public StructField(DavisType type, string name, int size, int offset)
		{
			Type = type;
			Name = name;
			this.size = size;
			this.offset = offset;
		}
	}
	internal class Variable : IEquatable<Variable>, IEquatable<string>
	{
		public string Name;
		public DavisType Type;

		public Variable(string name, DavisType type)
		{
			Name = name;
			Type = type;
		}

		public static bool operator ==(Variable a, string b) => a.Equals(b);
		public static bool operator !=(Variable a, string b) => !a.Equals(b);

		public static bool operator ==(Variable a, Variable b) => a.Equals(b);
		public static bool operator !=(Variable a, Variable b) => !a.Equals(b);

		public static bool operator ==(string a, Variable b) => b.Equals(a);
		public static bool operator !=(string a, Variable b) => !b.Equals(a);

		public bool Equals(Variable? other) => Name == other?.Name;
		public bool Equals(string? other) => Name == other;

		public override bool Equals(object? obj)
		{
			return Equals(obj as Variable);
		}
		public override int GetHashCode() => HashCode.Combine(Name, Type);
	}
	internal class Function : IEquatable<Function>, IEquatable<string>
	{
		public string Name;
		public List<(string, DavisType)> Arguments;

		public bool Equals(Function? other) => Name == other?.Name;
		public bool Equals(string? other) => Name.Equals(other);

		public static bool operator ==(Function a, string b) { return a.Equals(b); }
		public static bool operator !=(Function a, string b) { return !a.Equals(b); }

		public static bool operator ==(string a, Function b) { return b.Equals(a); }
		public static bool operator !=(string a, Function b) { return !b.Equals(a); }

		public static bool operator ==(Function a, Function b) { return a.Equals(b); }
		public static bool operator !=(Function a, Function b) { return !a.Equals(b); }

		public Function(string name, List<(string, DavisType)> arguments)
		{
			Name = name;
			Arguments = arguments;
		}

		public override bool Equals(object? obj)
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

		public FunctionStub(List<(string, DavisType)> arguments, DavisType returnType, string name)
		{
			Arguments = arguments;
			ReturnType = returnType;
			Name = name;
		}
	}

}
