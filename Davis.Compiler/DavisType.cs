using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davis.Compilation
{
	internal class DavisType
	{
		public bool Packed = false;
		public string Identifier;
		public IntrinsicType BaseType;
		public Dictionary<string, StructField> Fields = new();
		public int Size;

		public DavisType(string identifier, IntrinsicType baseType, Dictionary<string, StructField> fields, int size)
		{
			Identifier = identifier;
			BaseType = baseType;
			Fields = fields;
			Size = size;
		}

		public static readonly DavisType I8 = 
			new(
				"i8",
				IntrinsicType.I8,
				new(),
				1
			);

		public static readonly DavisType Char =
			new(
				"char",
				IntrinsicType.Char,
				new(),
				1
			);
	}

	internal enum IntrinsicType
	{ 
		I8,
		Char,
		Structure
	}
}
