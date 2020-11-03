using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.FT232H.I2C;

namespace LibApds9960
{
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
			Device.Write(Address, new[] { Registers[reg]});
			byte[] result = new byte[1];
			Device.Read(Address, result);
			return result[0];
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
			reg |= 0b10;
			WriteReg(Reg.ENABLE, reg);
		}

		public void DisalbeProximityMode()
		{
			byte reg = ReadReg(Reg.ENABLE);
			reg &= 0b1111_1101;
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
			// Set Gesture Mode (GMODE) to 1
			byte reg = ReadReg(Reg.GCONF4);
			reg |= 0b01;
			WriteReg(Reg.GCONF4, reg);

			// Set Gesture Direction Enable (GDIMS) to 1
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
			byte reg = ReadReg(Reg.GFLVL);
			return reg > 0;

			//byte reg = ReadReg(Reg.GSTATUS);
			//if ((reg & 1) == 1)
			//{
			//	return true;
			//}

			//return false;
		}

		public async Task<byte> ReadStatus()
		{
			return ReadReg(Reg.STATUS);
		}

		public async Task<byte> ReadGestureStatus()
		{
			return ReadReg(Reg.GSTATUS);
		}

		public async Task<byte[]> ReadAvailableGestures()
		{
			byte reg = ReadReg(Reg.GFLVL);
			byte[] buffer = new byte[reg * 4];
			Device.Write(Address, new[] { Registers[Reg.GFIFO_U] });
			Device.Read(Address, buffer);

			return buffer;
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
