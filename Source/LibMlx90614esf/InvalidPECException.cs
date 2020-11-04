using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LibMlx90614esf
{
	[Serializable]
	public class InvalidPECException : Exception
	{
		public InvalidPECException() : base()
		{
		}

		public InvalidPECException(string message) : base(message)
		{
		}

		public InvalidPECException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidPECException(SerializationInfo info, StreamingContext context) : base(info, context)
		{ }
	}
}
