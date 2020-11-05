using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public partial class GpioI2CDeviceBaseClass : II2C
	{
		public FtdiMpsseI2CResult Read(byte deviceAddress, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES)
		{
			return CheckResult(LibMpsse.I2C_DeviceRead(_i2cHandle, deviceAddress, sizeToTransfer, buffer, out sizeTransfered, (int)options));
		}

		public FtdiMpsseI2CResult Read(byte deviceAddress, byte[] buffer)
		{
			return Read(deviceAddress, buffer, buffer.Length, out int sizeTransfered);
		}

		public FtdiMpsseI2CResult Write(byte deviceAddress, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES)
		{
			return CheckResult(LibMpsse.I2C_DeviceWrite(_i2cHandle, deviceAddress, sizeToTransfer, buffer, out sizeTransfered, (int)options));
		}

		public FtdiMpsseI2CResult Write(byte deviceAddress, byte[] buffer)
		{
			return Write(deviceAddress, buffer, buffer.Length, out int sizeTransfered);
		}

		public bool Ok(FtdiMpsseI2CResult i2cResult) => i2cResult == FtdiMpsseI2CResult.Ok;
	}
}
