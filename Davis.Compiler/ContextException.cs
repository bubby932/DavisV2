using System;

namespace Davis.Compilation
{
	[Serializable]
	public class InvalidContextException : Exception
	{
		public InvalidContextException() { }
		public InvalidContextException(string message) : base(message) { }
		public InvalidContextException(string message, Exception inner) : base(message, inner) { }
		protected InvalidContextException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
