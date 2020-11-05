using FTD2XX_NET;
using MadeInTheUSB.FT232H;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H.I2C
{
	public partial class GpioI2CDeviceBaseClass : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead
	{
		protected const int gpioStartIndex = 0;
		protected const int maxGpio = 9;
		protected const int ValuesDefaultMask = 0;
		protected const int DirectionDefaultMask = 0xFF;

		private int Values;
		private int Directions;

		public IDigitalWriteRead GPIO => this;

		public II2C I2C => this as II2C;

		public byte GpioStartIndex => throw new NotImplementedException();

		public byte MaxGpio => maxGpio;

		protected I2C_CHANNEL_CONFIG Config { get; set; }

		public GpioI2CDeviceBaseClass(I2C_CHANNEL_CONFIG config, int channelIndex)
		{
			Config = config;
			InitLibAndHandle(channelIndex);
			GpioInit();
		}

		public void DigitalWrite(PinState mode, params int[] pins)
		{
			foreach (var p in pins)
				this.DigitalWrite(p, mode);
		}

		public bool IsGpioOn(int pin)
		{
			return DigitalRead(pin) == PinState.High;
		}

		public void SetPinMode(int pin, PinMode mode)
		{
			if (mode == PinMode.Output)
				Directions |= PowerOf2[pin];
			else
				Directions &= ~PowerOf2[pin];

			var result = LibMpsse.FT_WriteGPIO(_i2cHandle, (short)Directions, (short)Values);
			if (result != FtdiMpsseI2CResult.Ok)
			{
				throw new GpioException(result, nameof(SetPinMode));
			}
		}

		public void DigitalWrite(int pin, PinState state)
		{
			if (state == PinState.High)
				this.Values |= PowerOf2[pin];
			else
				Values &= ~PowerOf2[pin];

			var result = LibMpsse.FT_WriteGPIO(_i2cHandle, (short)Directions, (short)Values);
			if (result != FtdiMpsseI2CResult.Ok)
			{
				if (Debugger.IsAttached)
					Debugger.Break();

				throw new GpioException(result, nameof(DigitalWrite));
			}
		}

		public PinState DigitalRead(int pin)
		{
			var gpioMask = ReadGPIOMask();
			if (gpioMask == -1)
				return PinState.Unknown;

			return (gpioMask & PowerOf2[pin]) == PowerOf2[pin] ? PinState.High : PinState.Low;
		}

		public void SetGpioMask(byte mask)
		{
			var result = WriteGPIOMask(mask);
			if (result != FtdiMpsseI2CResult.Ok)
				throw new GpioException(result, nameof(SetGpioMask));
		}

		public byte GetGpioMask(bool forceRead = false)
		{
			var values = ReadGPIOMask();

			if (values == -1)
				throw new GpioException(FtdiMpsseI2CResult.IoError, nameof(GetGpioMask));

			return (byte)Values;
		}

		public void SetPullUp(int p, PinState d)
		{
			throw new NotImplementedException();
		}

		protected void GpioInit()
		{
			WriteGPIOMask(directions: DirectionDefaultMask, values: ValuesDefaultMask);
		}

		protected void InitLibAndHandle(int channelIndex)
		{
			if (_i2cHandle != IntPtr.Zero)
				return;

			LibMpsse.Init();

			var config = Config;

			var result = CheckResult(LibMpsse.I2C_OpenChannel(channelIndex, out _i2cHandle));

			if (_i2cHandle == IntPtr.Zero)
				throw new I2CNotConnectedException(FtdiMpsseI2CResult.InvalidAddress);


			result = CheckResult(LibMpsse.I2C_InitChannel(_i2cHandle, ref config));

			Config = config;
		}

		protected FtdiMpsseI2CResult WriteGPIOMask(byte values)
		{
			return LibMpsse.FT_WriteGPIO(_i2cHandle, 0, values);
		}

		protected FtdiMpsseI2CResult WriteGPIOMask(byte directions, byte values)
		{
			Values = values;
			Directions = directions;
			return LibMpsse.FT_WriteGPIO(_i2cHandle, directions, values);
		}

		protected int ReadGPIOMask()
		{
			int values;
			var result = LibMpsse_AccessToCppDll.FT_ReadGPIO(_i2cHandle, out values);

			if (result == FtdiMpsseSPIResult.Ok)
				return values;

			return -1;
		}

		protected FtdiMpsseI2CResult CheckResult(FtdiMpsseI2CResult result)
		{
			if (result != FtdiMpsseI2CResult.Ok)
				throw new I2CNotConnectedException(result);

			return FtdiMpsseI2CResult.Ok;
		}

		protected FtdiMpsseI2CResult CheckResult(FTD2XX_NET.FTDI.FT_STATUS status)
		{
			if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
			{
				throw new I2CNotConnectedException(Convert(status));
			}

			return FtdiMpsseI2CResult.Ok;
		}

		protected FtdiMpsseI2CResult Convert(FTDI.FT_STATUS status)
		{
			FtdiMpsseI2CResult result;

			switch (status)
			{
				case FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_FOUND:
					result = FtdiMpsseI2CResult.DeviceNotFound;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED:
					result = FtdiMpsseI2CResult.DeviceNotOpened;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_EEPROM_ERASE_FAILED:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_EEPROM_READ_FAILED:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_EEPROM_WRITE_FAILED:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
					result = FtdiMpsseI2CResult.IoError;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_INSUFFICIENT_RESOURCES:
					result = FtdiMpsseI2CResult.InsufficientResources;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_INVALID_ARGS:
				case FTD2XX_NET.FTDI.FT_STATUS.FT_INVALID_PARAMETER:
					result = FtdiMpsseI2CResult.InvalidParameter;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_INVALID_BAUD_RATE:
					result = FtdiMpsseI2CResult.InvalidBaudRate;
					break;
				case FTD2XX_NET.FTDI.FT_STATUS.FT_INVALID_HANDLE:
					result = FtdiMpsseI2CResult.InvalidAddress;
					break;
				default:
					result = FtdiMpsseI2CResult.DeviceNotOpened;
					break;
			}

			return result;
		}

		protected FtdiMpsseI2CResult Convert(FtdiMpsseSPIResult result)
		{
			switch(result)
			{
				case FtdiMpsseSPIResult.DeviceNotFound:
					return FtdiMpsseI2CResult.DeviceNotFound;
				case FtdiMpsseSPIResult.DeviceNotOpened:
					return FtdiMpsseI2CResult.DeviceNotOpened;
				case FtdiMpsseSPIResult.InsufficientResources:
					return FtdiMpsseI2CResult.InsufficientResources;
				case FtdiMpsseSPIResult.InvalidBaudRate:
					return FtdiMpsseI2CResult.InvalidBaudRate;
				case FtdiMpsseSPIResult.InvalidHandle:
					return FtdiMpsseI2CResult.InvalidAddress;
				case FtdiMpsseSPIResult.InvalidParameter:
					return FtdiMpsseI2CResult.InvalidParameter;
				case FtdiMpsseSPIResult.IoError:
					return FtdiMpsseI2CResult.IoError;
				case FtdiMpsseSPIResult.Ok:
					return FtdiMpsseI2CResult.Ok;
				default:
					return FtdiMpsseI2CResult.DeviceNotFound;
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				LibMpsse.Cleanup();

				disposedValue = true;
			}
		}

		~GpioI2CDeviceBaseClass()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
