using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public interface II2C
	{
		FtdiMpsseI2CResult Write(byte deviceAddress, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK);
		FtdiMpsseI2CResult Write(byte deviceAddress, byte[] buffer);
		FtdiMpsseI2CResult Read(byte deviceAddress, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK);
		FtdiMpsseI2CResult Read(byte deviceAddress, byte[] buffer);
		bool Ok(FtdiMpsseI2CResult i2cResult);
	}
}
