using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LibMlx90614esf
{
	[Serializable]
	public class TempSensorException : Exception
	{
		public TempSensorException() : base()
		{
		}

		public TempSensorException(string message) : base(message)
		{
		}

		public TempSensorException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TempSensorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{ }
	}
}
