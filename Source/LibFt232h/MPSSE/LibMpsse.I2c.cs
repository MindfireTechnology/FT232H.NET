using MadeInTheUSB.FT232H.I2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;

namespace MadeInTheUSB.FT232H
{
	internal partial class LibMpsse
	{
		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_GetNumChannels(out int result);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_GetChannelInfo(int index, out FT_DEVICE_LIST_INFO_NODE chainInfo);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_OpenChannel(int index, out IntPtr handle);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_InitChannel(IntPtr handle, ref I2C_CHANNEL_CONFIG config); // Note: we may not need ref on this param

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_CloseChannel(IntPtr handle);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_DeviceRead(IntPtr handle, int deviceAddress, int sizeToTransfer, byte[] buffer, out int sizeTransferred, int options);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult I2C_DeviceWrite(IntPtr handle, int deviceAddress, int sizeToTransfer, byte[] buffer, out int sizeTransferred, int options);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult FT_WriteGPIO(IntPtr handle, short dir, short value);

		[DllImport("libMPSSE.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public extern static FtdiMpsseI2CResult FT_ReadGPIO(IntPtr handle, out short value);
	}

	public struct FT_DEVICE_LIST_INFO_NODE
	{
		public uint Flags;
		public uint Type;
		public uint ID;
		public int LocId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string SerialNumber;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string Description;
		public IntPtr ftHandle;
	}

	public enum I2C_CLOCKRATE
	{
		I2C_CLOCK_SLOW_MODE = 10000,                            /* 10kb/sec */
		I2C_CLOCK_STANDARD_MODE = 100000,                       /* 100kb/sec */
		I2C_CLOCK_FAST_MODE = 400000,                           /* 400kb/sec */
		I2C_CLOCK_FAST_MODE_PLUS = 1000000,                     /* 1000kb/sec */
		I2C_CLOCK_HIGH_SPEED_MODE = 3400000                     /* 3.4Mb/sec */
	}

	public struct I2C_CHANNEL_CONFIG
	{
		public I2C_CLOCKRATE ClockRate;
		public byte LatencyTimer;
		public int Options;
	}
}
