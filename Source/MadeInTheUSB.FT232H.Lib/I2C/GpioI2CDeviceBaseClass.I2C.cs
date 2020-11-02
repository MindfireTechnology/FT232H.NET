using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public partial class GpioI2CDeviceBaseClass
	{
		public FtdiMpsseI2CResult Write(uint device, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.None)
		{
			var result = CheckResult(LibMpsse.I2C_DeviceWrite(_i2cHandle, (int)device, sizeToTransfer, buffer, out sizeTransfered, (int)options));
			return result;
		}

		public FtdiMpsseI2CResult Write(uint device, byte[] buffer)
		{
			return Write(device, buffer, buffer.Length, out int sizeTransfered, FtI2CTransferOptions.None);
		}

		public FtdiMpsseI2CResult Read(uint device, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.None)
		{
			return CheckResult(LibMpsse.I2C_DeviceRead(_i2cHandle, (int)device, sizeToTransfer, buffer, out sizeTransfered, (int)options));
		}

		public FtdiMpsseI2CResult Read(uint device, byte[] buffer)
		{
			return Read(device, buffer, buffer.Length, out int sizeTransfered, FtI2CTransferOptions.None);
		}

		public bool Ok(FtdiMpsseI2CResult i2cResult) => i2cResult == FtdiMpsseI2CResult.Ok;
	}
}
