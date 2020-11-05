using FTD2XX_NET;
using LibMlx90614esf;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.FT232H.I2C;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using static System.Console;

namespace I2CTestApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var ft232Device = FT232HDetector.Detect();

			if (ft232Device.Ok)
				Console.WriteLine(ft232Device.ToString());
			else
				throw new Exception("Could not find a device");

			var i2cDevice = new GpioI2CDevice(new I2C_CHANNEL_CONFIG
			{
				ClockRate = I2C_CLOCKRATE.I2C_CLOCK_FAST_MODE,
				LatencyTimer = 1
			}, channelIndex: 0);

			try
			{
				var gpios = i2cDevice.GPIO;
				await Blink(gpios, 4, 5, 250);

				var tempSensor = new Mlx90614(i2cDevice, 0x2A) { TempInF = true };

				// Trying to figure out sane values for these:
				gsensor.SetGestureThreshold(0x30, 0x20, 0b11);
				gsensor.SetGainAndIrIntensity(0b11 /* Full Gain */, 0b11 /* Full Intensity */);

				do
				{
					while (!gsensor.GestureAvailable())
					{
						//byte status = await gsensor.ReadStatus();
						//byte gstatus = await gsensor.ReadGestureStatus();
						//Debug.WriteLine($"Status: {status.ToString("X")}; GStatus: {gstatus.ToString("X")}");
						await Task.Delay(10);
					}

					//Console.WriteLine("Gesture is available!");
					//byte[] data = await gsensor.ReadAvailableGestureData();
					//string hexstring = BytesToHexString(data);
					//WriteLine($"Gesture Data:\n{hexstring}");

					var result = await gsensor.ReadGesture();
					WriteLine(result);

					//await Task.Delay(1000);
					//Read(i2cDevice).Wait();
				} while (true);
				//try
				//{
				//	await tempSensor.SetAddress(0x2A);
				//}
				//catch { }

				var temperatures = new List<string>();

				for (int i = 0; i < 100; i++)
				{
					var ambientTemp = await tempSensor.ReadAmbientTemperature();
					var objectTemp = await tempSensor.ReadObjectTemperature();

					Console.WriteLine($"Ambient: {ambientTemp}, Object: {objectTemp}");
					temperatures.Add($"Ambient: {ambientTemp}, Object: {objectTemp}");

					await Task.Delay(1000);
				}
			}
			catch (Exception e)
			{
				WriteLine(e);
			}
			WriteLine("Press any key to exit...");
			Console.Read();
		}

		private static string BytesToHexString(byte[] data)
		{
			StringBuilder sb = new StringBuilder();
			int bytes = 0;
			foreach (var b in data)
			{
				sb.Append(b.ToString("X").PadLeft(2, '0') + ' ');
				if (++bytes % 4 == 0)
					sb.AppendLine();
			}

			sb.AppendLine();

			return sb.ToString();
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
			byte[] buffer = new byte[10];
			device.Read(0x39, buffer);
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

		//       LibMpsse.I2C_OpenChannel(channel, out IntPtr device)

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
		//            CheckResult(LibMpsse.I2C_DeviceRead(device, 0x40, 1, buffer, out sizeTransferred, (int)(FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES))));
		//        }
		//        catch (InvalidOperationException ex)
		//        {
		//            WriteLine(ex.ToString());
		//        }

		//        try
		//        {
		//            CheckResult(LibMpsse.I2C_DeviceWrite(device, 0x40, 1, new byte[] { 0xA5 }, out sizeTransferred, (int)(FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES))));
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
