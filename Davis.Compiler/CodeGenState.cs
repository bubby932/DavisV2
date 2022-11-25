using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davis.Compilation
{
	internal class CodeGenState
	{
		public CodeGenContext Context = CodeGenContext.File;

		public object? Scope = null;
		public string? EntryPoint = null;

		public int LocalOffset = 0;
		public Dictionary<string, (int, Variable)> Locals = new();
		public bool EmittedReturnInstruction = false;

		public Dictionary<string, DavisType> Types = new()
		{
			{ "i8", DavisType.I8 },
			{ "char", DavisType.Char }
		};

		public Dictionary<string, Function> Functions = new()
		{

		};

		public Dictionary<string, DavisType> Globals = new()
		{

		};

		public bool RegisterType(DavisType type)
		{
			if (Types.ContainsKey(type.Identifier)) return false;

			Types.Add(type.Identifier, type);
			return true;
		}

		public bool RegisterFunction(Function decl)
		{
			if (Functions.ContainsKey(decl.Name)) return false;

			Functions.Add(decl.Name, decl);
			return true;
		}

		public bool RegisterGlobal(string Identifier, DavisType type)
		{
			if (Globals.ContainsKey(Identifier)) return false;

			Globals.Add(Identifier, type);
			return true;
		}

		public void UpdateContext(CodeGenContext ctx, object? scope)
		{
			Context = ctx;
			Scope = scope;
		}

		public void SetEntryPoint(string name)
		{
			if (EntryPoint != null) throw new Exception("Entry point already defined!");
			EntryPoint = name;
		}

		public static implicit operator CodeGenContext(CodeGenState self) => self.Context;
	}


	internal enum CodeGenContext
	{
		File,
		Structure,
		Function
	}
}
