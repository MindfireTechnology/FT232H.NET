using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public interface II2C
	{
		FtdiMpsseI2CResult Write(uint device, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options = FtI2CTransferOptions.None);
		FtdiMpsseI2CResult Write(uint device, byte[] buffer);
		FtdiMpsseI2CResult Read(uint device, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions optsion = FtI2CTransferOptions.None);
		FtdiMpsseI2CResult Read(uint device, byte[] buffer);
		bool Ok(FtdiMpsseI2CResult i2cResult);
	}
}
