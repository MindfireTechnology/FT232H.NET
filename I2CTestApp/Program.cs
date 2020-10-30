using FTD2XX_NET;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.FT232H.I2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using static System.Console;

namespace I2CTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var ft232Device = FT232HDetector.Detect();

			if (ft232Device.Ok)
				Console.WriteLine(ft232Device.ToString());
			else
				throw new Exception("Could not find a device");

			var i2cDevice = new GpioI2CDevice(new I2C_CHANNEL_CONFIG
			{
				ClockRate = I2C_CLOCKRATE.I2C_CLOCK_STANDARD_MODE,
				LatencyTimer = 255
			}, 0);

			try
			{
				var gpios = i2cDevice.GPIO;
				Blink(gpios, 4, 5, 250).Wait();

				Read(i2cDevice).Wait();
			}
			catch (Exception e)
			{
				WriteLine(e);
				Console.Read();
			}
		}

		public static async Task BlinkAll(IDigitalWriteRead gpios, int times = 1000, int delay = 500)
		{
			for (int i = 0; i < times; i++)
			{
				for (int pin = 0; pin < gpios.MaxGpio; pin++)
				{
					gpios.DigitalWrite(pin, PinState.High);
				}

				await Task.Delay(delay);
				for (int pin = 0; pin < gpios.MaxGpio; pin++)
					gpios.DigitalWrite(pin, PinState.Low);

				await Task.Delay(delay);
			}
		}

		public static async Task Blink(IDigitalWriteRead gpios, int pin = 4, int times = 1000, int delay = 500)
		{
			for (int i = 0; i < times; i++)
			{
				gpios.DigitalWrite(pin, PinState.High);

				await Task.Delay(delay);

				gpios.DigitalWrite(pin, PinState.Low);

				await Task.Delay(delay);
			}
		}

		public static async Task SetDeviceAddress(GpioI2CDevice device)
		{
			// 1. Write an all zero slave address, followed by a LOW R/W bit
			byte[] data = new byte[] { 0x0, 0x0E,  };
			device.Write(0, data);

			// 2.
		}

		public static async Task Read(GpioI2CDevice device)
		{
			//for(uint i = 0; i < 255; i++)
			//{
			//	try
			//	{
			//		var exists = device.Write(i, new byte[] { 0b0010_1110 });

			//		if (exists == FtdiMpsseI2CResult.Ok)
			//			WriteLine($"Device found at address: {i}");
			//	}
			//	catch (I2CNotConnectedException ince)
			//	{

			//	}
			//}

			uint address = 0x39;
			var result = device.Write(address, new byte[] { 0x80 });

			byte[] data = new byte[10];
			result = device.Read(address, data, 2, out int size);
			Console.WriteLine($"Read {size} bytes: {data}");
		}

		//static void Main(string[] args)
		//{
		//    LibMpsse.Init();

		//    CheckResult(LibMpsse.I2C_GetNumChannels(out int channels));

		//    for (int channel = 0; channel < channels; channel++)
		//    {
		//        CheckResult(LibMpsse.I2C_GetChannelInfo(channel, out FT_DEVICE_LIST_INFO_NODE info));
		//        WriteLine($"Channel {channel}:");
		//        WriteLine($"Flags: {info.Flags}; Type: {info.Type}");

		//        CheckResult(LibMpsse.I2C_OpenChannel(channel, out IntPtr device));

		//        I2C_CHANNEL_CONFIG config = new I2C_CHANNEL_CONFIG
		//        {
		//            ClockRate = I2C_CLOCKRATE.I2C_CLOCK_FAST_MODE,
		//            LatencyTimer = 255
		//        };

		//        CheckResult(LibMpsse.I2C_InitChannel(device, ref config));

		//        // Read & Write To/From Channel
		//        byte[] buffer = new byte[1];
		//        int sizeTransferred;
		//        try
		//        {
		//            CheckResult(LibMpsse.I2C_DeviceRead(device, 0x40, 1, buffer, out sizeTransferred, 0));
		//        }
		//        catch (InvalidOperationException ex)
		//        {
		//            WriteLine(ex.ToString());
		//        }

		//        try
		//        {
		//            CheckResult(LibMpsse.I2C_DeviceWrite(device, 0x40, 1, new byte[] { 0xA5 }, out sizeTransferred, 0));
		//        }
		//        catch (InvalidOperationException ex)
		//        {
		//            WriteLine(ex.ToString());
		//        }

		//        CheckResult(LibMpsse.I2C_CloseChannel(device));
		//    }

		//    LibMpsse.Cleanup();
		//}

		//private static void CheckResult(FTDI.FT_STATUS fT_STATUS)
		//{
		//    if (fT_STATUS != FTDI.FT_STATUS.FT_OK)
		//        throw new InvalidOperationException("Result from function call was: " + fT_STATUS.ToString());
		//}
	}
}
