using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.FT232H.I2C;

namespace LibApds9960
{
	[Flags]
	public enum Gesture
	{
		Unrecongnized = 0,
		LeftToRight = 1,
		RightToLeft = 2,
		TopToBottom = 4,
		BottomToTop = 8,
		Bop = 16,
		PullAway = 32
	}

    public class Apds9660GestureSensor : IDisposable
    {
		public byte Address { get; set; } = 0x39;
		public II2C Device { get; }
		public IDigitalWriteRead Gpio { get; }
		public int? InterruptPin { get; }
		protected Dictionary<string, byte> Registers = new Dictionary<string, byte>
		{
			{"ENABLE",     0x80},
			{"ATIME",      0x81},
			{"WTIME",      0x83},
			{"AILTL",      0x84},
			{"AILTH",      0x85},
			{"AIHTL",      0x86},
			{"AIHTH",      0x87},
			{"PILT",       0x89},
			{"PIHT",       0x8B},
			{"PERS",       0x8C},
			{"CONFIG1",    0x8D},
			{"PPULSE",     0x8E},
			{"CONTROL",    0x8F},
			{"CONFIG2",    0x90},
			{"ID",         0x92},
			{"STATUS",     0x93},
			{"CDATAL",     0x94},
			{"CDATAH",     0x95},
			{"RDATAL",     0x96},
			{"RDATAH",     0x97},
			{"GDATAL",     0x98},
			{"GDATAH",     0x99},
			{"BDATAL",     0x9A},
			{"BDATAH",     0x9B},
			{"PDATA",      0x9C},
			{"POFFSET_UR", 0x9D},
			{"POFFSET_DL", 0x9E},
			{"CONFIG3",    0x9F},
			{"GPENTH",     0xA0},
			{"GEXTH",      0xA1},
			{"GCONF1",     0xA2},
			{"GCONF2",     0xA3},
			{"GOFFSET_U",  0xA4},
			{"GOFFSET_D",  0xA5},
			{"GPULSE",     0xA6},
			{"GOFFSET_L",  0xA7},
			{"GOFFSET_R",  0xA9},
			{"GCONF3",     0xAA},
			{"GCONF4",     0xAB},
			{"GFLVL",      0xAE},
			{"GSTATUS",    0xAF},
			{"IFORCE",     0xE4},
			{"PICLEAR",    0xE5},
			{"CICLEAR",    0xE6},
			{"AICLEAR",    0xE7},
			{"GFIFO_U",    0xFC},
			{"GFIFO_D",    0xFD},
			{"GFIFO_L",    0xFE},
			{"GFIFO_R",    0xFF}
		};

		public Apds9660GestureSensor(II2C device, IDigitalWriteRead gpio = null, int? interruptPin = null)
		{
			Device = device;
			Gpio = gpio;
			InterruptPin = interruptPin;
		}

		protected virtual byte ReadReg(string reg)
		{
			byte[] result = new byte[1];
			Device.Write(Address, new[] { Registers[reg]}, 1, out int sizetransferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES);
			Device.Read(Address, result, 1, out sizetransferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES);
			return result[0];
		}

		protected virtual byte[] ReadRegBytes(string reg, int length)
		{
			byte[] result = new byte[length];
			Device.Write(Address, new[] { Registers[reg] }, 1, out int sizetransferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES);
			Device.Read(Address, result, length, out sizetransferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES);
			return result;
		}

		protected virtual void WriteReg(string reg, byte value)
		{
			Device.Write(Address, new[] { Registers[reg], value });
		}

		public Task Init()
		{
			return Task.Run(async () =>
			{
				byte id = ReadReg(Reg.ID);
				if (id != 0xAB)
					throw new UnexpectedIdException($"Unexpected ID! Expected 0xAB, but got 0x{id.ToString("X").PadLeft(2, '0')}");

				Disable();
				WriteReg(Reg.WTIME, 0xFF);
				WriteReg(Reg.GPULSE, 0x8f); // 16us, 16 pulses // default is: 0x40 = 8us, 1 pulse
				WriteReg(Reg.PPULSE, 0x8f); // 16us, 16 pulses // default is: 0x40 = 8us, 1 pulse
				EnableGestureInterrupt();
				EnableGestureMode();
				EnableProximityMode();
				Enable();
				EnableWaitMode();
				WriteReg(Reg.ATIME, (byte)(256 - (10 / 2.78)));
				WriteReg(Reg.CONTROL, 0x2);
				await Task.Delay(10);

				Enable();
			});
		}

		public void Enable()
		{
			byte val = ReadReg(Reg.ENABLE);
			val |= 0b1;
			WriteReg(Reg.ENABLE, val);
		}

		public void Disable()
		{
			byte val = ReadReg(Reg.ENABLE);
			val &= 0b1111_1110;
			WriteReg(Reg.ENABLE, val);
		}

		public void PowerOn()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg |= 0x1;
			WriteReg(Reg.ENABLE, reg);
		}

		public void EnableProximityMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg |= 0b100;
			WriteReg(Reg.ENABLE, reg);
		}

		public void DisalbeProximityMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg &= 0b1111_1011;
			WriteReg(Reg.ENABLE, reg);
		}

		public void EnableGestureInterrupt()
		{
			byte reg = ReadReg(Reg.GCONF4);
			reg |= 0b10;
			WriteReg(Reg.GCONF4, reg);
		}

		public void DisableGestureInterrupt()
		{
			byte reg = ReadReg(Reg.GCONF4);
			reg &= 0b1111_1101;
			WriteReg(Reg.GCONF4, reg);
		}

		public void EnableGestureMode()
		{
			byte reg;

			// Set Gesture Mode (GMODE) to 1
			//reg = ReadReg(Reg.GCONF4);
			//reg |= 0b01;
			//WriteReg(Reg.GCONF4, reg);

			// Set Gesture Direction Enable (GDIMS) to 0b11 (both directions)
			reg = ReadReg(Reg.GCONF3);
			reg |= 0b11;
			WriteReg(Reg.GCONF3, reg);

			// Set Gesture Enable
			reg = ReadReg(Reg.ENABLE);
			reg |= 0b0100_0000;
			WriteReg(Reg.ENABLE, reg);
		}

		public void DisableGestureMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg &= 0b1011_1111;
			WriteReg(Reg.ENABLE, reg);
		}

		public void EnableColorMode()
		{

		}

		public void DisableColorMode()
		{

		}

		public void EnableWaitMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg |= 0b0000_1000;
			WriteReg(Reg.ENABLE, reg);
		}

		public void DisableWaitMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg &= 0b1111_0111;
			WriteReg(Reg.ENABLE, reg);
		}

		public void Sleep()
		{

		}


		public void SetWaitTime(TimeSpan delay)
		{

		}

		public bool GestureAvailable()
		{
			// Check for the gesture interrupt
			byte reg = ReadReg(Reg.STATUS);
			return (reg & 0b000_0100) > 0;

			//byte reg = ReadReg(Reg.GSTATUS);
			//if ((reg & 1) == 1)
			//{
			//	return true;
			//}

			//reg = ReadReg(Reg.GCONF4);
			//reg &= 0b01;
			//Debug.WriteLine($"GMODE = {reg > 0}");

			return false;
		}

		public void SetGestureThreshold(byte highLevel, byte lowLevel, byte gestureRecognizeMin)
		{
			WriteReg(Reg.GPENTH, highLevel);
			WriteReg(Reg.GEXTH, lowLevel);
			byte reg = ReadReg(Reg.GCONF1);
			reg |= (byte)(0b0000_0011 & gestureRecognizeMin);
			WriteReg(Reg.GCONF1, reg);
		}

		public void SetGainAndIrIntensity(byte gain, byte irIntensity)
		{
			//GGAIN & GLDRIVE
			var reg = ReadReg(Reg.GCONF2);
			reg |= (byte)((0b1100_0000 & (gain << 6)) | (0b0011_0000 & (irIntensity << 4)));
			WriteReg(Reg.CONFIG2, reg);
		}

		public async Task<byte> ReadStatus()
		{
			return ReadReg(Reg.STATUS);
		}

		public async Task<byte> ReadGestureStatus()
		{
			return ReadReg(Reg.GSTATUS);
		}

		public async Task<byte[]> ReadAvailableGestureData()
		{
			byte reg = ReadReg(Reg.GFLVL);
			if (reg == 0)
				return new byte[0];

			byte[] buffer = new byte[reg * 4];
			Device.Write(Address, new[] { Registers[Reg.GFIFO_U] });
			Device.Read(Address, buffer);

			return buffer;
		}

		public async ValueTask<Gesture> ReadGesture()
		{
			// A breif delay to wait for all of the buffer data to be there
			//byte[] values  = await ReadAvailableGestureData();

			//await Task.Delay(200);
			//byte[] values2 = await ReadAvailableGestureData();

			//byte[] values = new byte[values1.Length + values2.Length];
			//Array.Copy(values1, values, values1.Length);
			//Array.Copy(values2, 0, values, values1.Length, values2.Length);

			// Wait a maximum of 700ms for hopefully 32 sets of data
			List<byte> bytes = new List<byte>();
			Stopwatch sw = Stopwatch.StartNew();
			int totalreads = 0;
			do
			{
				bytes.AddRange(await ReadAvailableGestureData());
				sw.Restart();
				totalreads++;
				await Task.Delay(50);
			} while (bytes.Count < 4 * 32 && sw.ElapsedMilliseconds < 600);

			byte[] values = bytes.ToArray();
			Debug.WriteLine($"{totalreads} Reads - Gesture Buffer({values.Length / 4}): {string.Join(", ", values.Select(n => "0x" + n.ToString("X").PadLeft(2, '0')))}");

			var result = Gesture.Unrecongnized;

			int samples = values.Length / 4;
			if (samples < 8)
				return Gesture.Unrecongnized;

			byte[] channelUp = values.Where((x, i) => i == 0 || i % 4 == 0).ToArray();
			byte[] channelDown = values.Skip(1).Where((x, i) => i == 0 || i % 4 == 0).ToArray();
			byte[] channelLeft = values.Skip(2).Where((x, i) => i == 0 || i % 4 == 0).ToArray();
			byte[] channelRight = values.Skip(3).Where((x, i) => i == 0 || i % 4 == 0).ToArray();

			// Search for the peaks in the data
			int channelUpPeakIndex = IndexOfMax(channelUp, channelUp.Max());
			int channelDownPeakIndex = IndexOfMax(channelDown, channelDown.Max());
			int channelLeftPeakIndex = IndexOfMax(channelLeft, channelLeft.Max());
			int channelRightPeakIndex = IndexOfMax(channelRight, channelRight.Max());

			// Evalueate each curve to see if there is anything to look at
			double centerpointRatioUp = Math.Abs(channelUpPeakIndex / (double)samples - 0.5);
			double centerpointRatioDown = Math.Abs(channelDownPeakIndex / (double)samples - 0.5);
			double centerpointRatioLeft = Math.Abs(channelLeftPeakIndex / (double)samples - 0.5);
			double centerpointRatioRight = Math.Abs(channelRightPeakIndex / (double)samples - 0.5);

			if (centerpointRatioUp < 0.3 && centerpointRatioDown < 0.3)
			{
				int upDownDiff = channelUpPeakIndex - channelDownPeakIndex;

				if (upDownDiff > 0 && upDownDiff >= 3)
					result |= Gesture.BottomToTop;
				else if (upDownDiff < 0 && (upDownDiff * -1) >= 3)
					result |= Gesture.TopToBottom;
			}

			if (centerpointRatioLeft < 0.3 && centerpointRatioRight < 0.3)
			{
				int leftRightDiff = channelLeftPeakIndex - channelRightPeakIndex;

				if (leftRightDiff > 0 && leftRightDiff >= 3)
					result |= Gesture.RightToLeft;
				else if (leftRightDiff < 0 && (leftRightDiff * -1) >= 3)
					result |= Gesture.LeftToRight;
			}

			return result;
		}

		protected int IndexOfMax(byte[] values, byte max)
		{
			for (int index = 0; index < values.Length; index++)
			{
				if (values[index] == max)
					return index;
			}

			return -1;
		}

		protected class Reg
		{
			public static string ENABLE = "ENABLE";
			public static string ATIME = "ATIME";
			public static string WTIME = "WTIME";
			public static string AILTL = "AILTL";
			public static string AILTH = "AILTH";
			public static string AIHTL = "AIHTL";
			public static string AIHTH = "AIHTH";
			public static string PILT = "PILT";
			public static string PIHT = "PIHT";
			public static string PERS = "PERS";
			public static string CONFIG1 = "CONFIG1";
			public static string PPULSE = "PPULSE";
			public static string CONTROL = "CONTROL";
			public static string CONFIG2 = "CONFIG2";
			public static string ID = "ID";
			public static string STATUS = "STATUS";
			public static string CDATAL = "CDATAL";
			public static string CDATAH = "CDATAH";
			public static string RDATAL = "RDATAL";
			public static string RDATAH = "RDATAH";
			public static string GDATAL = "GDATAL";
			public static string GDATAH = "GDATAH";
			public static string BDATAL = "BDATAL";
			public static string BDATAH = "BDATAH";
			public static string PDATA = "PDATA";
			public static string POFFSET_UR = "POFFSET_UR";
			public static string POFFSET_DL = "POFFSET_DL";
			public static string CONFIG3 = "CONFIG3";
			public static string GPENTH = "GPENTH";
			public static string GEXTH = "GEXTH";
			public static string GCONF1 = "GCONF1";
			public static string GCONF2 = "GCONF2";
			public static string GOFFSET_U = "GOFFSET_U";
			public static string GOFFSET_D = "GOFFSET_D";
			public static string GPULSE = "GPULSE";
			public static string GOFFSET_L = "GOFFSET_L";
			public static string GOFFSET_R = "GOFFSET_R";
			public static string GCONF3 = "GCONF3";
			public static string GCONF4 = "GCONF4";
			public static string GFLVL = "GFLVL";
			public static string GSTATUS = "GSTATUS";
			public static string IFORCE = "IFORCE";
			public static string PICLEAR = "PICLEAR";
			public static string CICLEAR = "CICLEAR";
			public static string AICLEAR = "AICLEAR";
			public static string GFIFO_U = "GFIFO_U";
			public static string GFIFO_D = "GFIFO_D";
			public static string GFIFO_L = "GFIFO_L";
			public static string GFIFO_R = "GFIFO_R";
		}

		#region IDisposable
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Apds9660GestureSensor()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
