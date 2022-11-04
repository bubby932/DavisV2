using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davis.Compilation
{
	internal class BlockContext
	{
		public BlockMode Mode;
		public string Label;

		public BlockContext(BlockMode mode, string label)
		{
			Mode = mode;
			Label = label;
		}
	}

	internal enum BlockMode
	{
		ControlFlow,
		Function,
		Struct
	}
}
