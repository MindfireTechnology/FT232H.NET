using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public enum FtdiMpsseI2CResult
	{
		Ok = 0,
		InvalidAddress,
		DeviceNotFound,
		DeviceNotOpened,
		IoError,
		InsufficientResources,
		InvalidParameter,
		InvalidBaudRate,
		NotOpenedForErase,
		NotOpenedForWrite,
		ReadFailed,
		WriteFailed,
		EraseFailed,
		NotPresent,
		NotProgrammed,
		InvalidArgs,
		Other
	}
}
