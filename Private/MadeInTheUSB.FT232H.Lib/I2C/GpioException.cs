using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	[Serializable]
	public class GpioException : Exception
	{
		public FtdiMpsseI2CResult Reason { get; private set; }
		public GpioException(FtdiMpsseI2CResult res, string message) : base($"GPIO operation failed, {message}. code:{res}")
		{
			Reason = res;
		}

		protected GpioException(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
