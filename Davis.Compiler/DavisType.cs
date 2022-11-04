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
		public Dictionary<string, DavisType> Fields = new();
		public int Size;

		public DavisType(string identifier, IntrinsicType baseType, Dictionary<string, DavisType> fields, int size)
		{
			Identifier = identifier;
			BaseType = baseType;
			Fields = fields;
			Size = size;
		}

		public DavisType(string identifier, IntrinsicType baseType, Dictionary<string, DavisType> fields)
		{
			Identifier = identifier;
			BaseType = baseType;
			Fields = fields;

			Size = 0;

			foreach (var item in fields)
			{
				Size += item.Value.Size;
			}
		}

		public static readonly DavisType I32 = 
			new(
				"i32",
				IntrinsicType.I32,
				new Dictionary<string, DavisType>(),
				4
			);

		public static readonly DavisType F32 =
			new(
				"f32",
				IntrinsicType.F32,
				new Dictionary<string, DavisType>(),
				4
			);

		public static readonly DavisType Char =
			new(
				"char",
				IntrinsicType.Char,
				new Dictionary<string, DavisType>(),
				1
			);
	}

	internal enum IntrinsicType
	{ 
		I32,
		F32,
		Char,
		Structure
	}
}
