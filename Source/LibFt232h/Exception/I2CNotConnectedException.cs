using MadeInTheUSB.FT232H.I2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H
{
	[Serializable]
	public class I2CNotConnectedException : Exception
	{
		public FtdiMpsseI2CResult Reason { get; }

		public I2CNotConnectedException(FtdiMpsseI2CResult reason)
		{
			Reason = reason;
		}

		public I2CNotConnectedException(FtdiMpsseI2CResult reason, string message) : base(message)
		{
			Reason = reason;
		}

		public I2CNotConnectedException(FtdiMpsseI2CResult reason, string message, System.Exception innerException) : base(message, innerException)
		{
			Reason = reason;
		}

		protected I2CNotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
