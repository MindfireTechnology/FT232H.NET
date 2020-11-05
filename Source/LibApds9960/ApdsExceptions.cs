using System;
using System.Collections.Generic;
using System.Text;

namespace LibApds9960
{

	[Serializable]
	public class ApdsException : Exception
	{
		public ApdsException() { }
		public ApdsException(string message) : base(message) { }
		public ApdsException(string message, Exception inner) : base(message, inner) { }
		protected ApdsException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}


	[Serializable]
	public class UnexpectedIdException : ApdsException
	{
		public UnexpectedIdException() { }
		public UnexpectedIdException(string message) : base(message) { }
		public UnexpectedIdException(string message, Exception inner) : base(message, inner) { }
		protected UnexpectedIdException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
