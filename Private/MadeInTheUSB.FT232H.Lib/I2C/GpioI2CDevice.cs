using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public class GpioI2CDevice : GpioI2CDeviceBaseClass
	{
		public GpioI2CDevice(I2C_CHANNEL_CONFIG config, int channelIndex) : base(config, channelIndex)
		{
		}
	}
}
