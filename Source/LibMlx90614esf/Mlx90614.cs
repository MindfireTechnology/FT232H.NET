using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.FT232H.I2C;

namespace LibMlx90614esf
{
	public class Mlx90614 : IDisposable
	{
		protected class RegisterAddress
		{
			public const byte MaxTempRange = 0;
			public const byte MinTempRange = 0x01;
			public const byte PwmControl = 0x02;
			public const byte TempRange = 0x03;
			public const byte Emissivity = 0x04;
			public const byte ConfigRegister1 = 0x05;
			public const byte SmbusAddress = 0x0E;
			public const byte ID1 = 0x1C;
			public const byte ID2 = 0x1D;
			public const byte ID3 = 0x1E;
			public const byte ID4 = 0x1F;
		}

		protected class RamAddress
		{
			public const byte RawIr1 = 0x04;
			public const byte RawIr2 = 0x05;
			public const byte Ta = 0x06;
			public const byte Tobj1 = 0x07;
			public const byte Tobj2 = 0x08;
		}

		public byte Address { get; protected set; }

		public bool TempInF { get; set; }

		protected II2C Device { get; }

		public Mlx90614(II2C device, byte address = 0)
		{
			Device = device;
			Address = address;
		}

		public Task EnableI2CMode()
		{
			return WriteEeprom(RegisterAddress.ConfigRegister1, new byte[] { 0x0 }, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT);
		}

		public Task EnablePWMMode()
		{
			return WriteEeprom(RegisterAddress.ConfigRegister1, new byte[] { 0x0 }, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT);
		}

		public Task Sleep()
		{
			return WriteEeprom(0xFF, new byte[] { 0 });
		}

		public Task<short> ReadId(int id)
		{
			var defaults = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT;

			return Task.Run(() =>
			{
				byte idLocation;
				switch (id)
				{
					case 1:
						idLocation = RegisterAddress.ID1;
						break;
					case 2:
						idLocation = RegisterAddress.ID2;
						break;
					case 3:
						idLocation = RegisterAddress.ID3;
						break;
					case 4:
					default:
						idLocation = RegisterAddress.ID4;
						break;
				}

				byte eeprom = (byte)(0x20 | idLocation);
				var result = Device.Write(Address, new byte[] { eeprom }, 1, out int transfered, defaults);
				if (!Device.Ok(result))
					throw new TempSensorException("Could not setup write for read opoeration");

				var data = new byte[3];
				result = Device.Read(Address, data, 3, out transfered, defaults | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT);

				if (!Device.Ok(result))
					throw new TempSensorException("Could not setup write for read opoeration");

				//byte crc = PEC.Calculate(data.Take(2));
				//if (crc != data[2])
				//{
				//	throw new InvalidPECException($"PEC did not match. Expected {data[2]}, but calculated {crc}");
				//}

				short combinedData = (short)((data[0] << 8) | data[1]);

				return combinedData;
			});
		}

		public Task SetAddress(byte address)
		{
			// Sequence for writing a value:
			// 1. Power up device
			// 2. Write 0x0000 into the cell of interest
			// 3. Wait at least 5ms
			// 4. Write the new value
			// 5. Wait at least 5ms
			// 6. Read back and compare if the write was successful
			// 7. Power down (to make sure the changes take place at next power up

			var defaults = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT;

			return Task.Run(async () =>
			{
				byte eepromLocation = (byte)(0x20 | RegisterAddress.SmbusAddress);
				var pec = PEC.Calculate(0, eepromLocation, 0, 0);
				var data = new byte[] { eepromLocation, 0, 0, pec };

				var result = Device.Write(0, data, data.Length, out int transfered, defaults | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT);
				if (result != FtdiMpsseI2CResult.Ok)
					throw new TempSensorException("Could not write 0 into bus address");
				await Task.Delay(12);

				pec = PEC.Calculate(0, eepromLocation, address, 0);
				data = new byte[] { eepromLocation, address, 0, pec };
				result = Device.Write(0, data, data.Length, out transfered, defaults | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT);
				if (!Device.Ok(result))
					throw new TempSensorException("Could not set new address");
				await Task.Delay(12);

				var readData = new byte[3];
				data = new byte[] { eepromLocation };
				result = Device.Write(0, data, data.Length, out transfered, defaults);
				result = Device.Read(0, readData, 3, out transfered, defaults | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT);
				if (!Device.Ok(result))
					throw new TempSensorException("Could not read I2C address");

				if (readData[0] != address)
					throw new TempSensorException("Temperature sensor address was not set successfully");

				Address = address;
			});
		}

		/// <summary>
		/// Read the temperature of the sensor itself
		/// </summary>
		/// <returns></returns>
		public async Task<double> ReadAmbientTemperature()
		{
			double tempInKelvin = await ReadTemp(RamAddress.Ta);

			double tempInC = ConvertKelvinToC(tempInKelvin);

			if (TempInF)
			{
				return ConvertCtoF(tempInC);
			}

			return tempInC;
		}

		/// <summary>
		/// Read the temperature being sensed in front of the sensor
		/// </summary>
		/// <returns></returns>
		public async Task<double> ReadObjectTemperature()
		{
			double value1 = await ReadTemp(RamAddress.Tobj1);
			double value2 = await ReadTemp(RamAddress.Tobj2);

			double temp1InC = ConvertKelvinToC(value1);
			double temp2InC = ConvertKelvinToC(value2);

			double temp = 0;

			if (temp1InC > 200 || temp1InC < -30)
				temp = temp2InC;
			else
				temp = temp1InC;

			if (TempInF)
			{
				return ConvertCtoF(temp);
			}

			return temp;
		}

		protected Task<double> ReadTemp(byte command)
		{
			return Task.Run(() =>
			{
				var result = Device.Write(Address, new byte[] { command }, 1, out int transferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT);

				if (!Device.Ok(result))
					throw new TempSensorException("Could not setup Ta read");

				var taData = new byte[3];
				result = Device.Read(Address, taData, taData.Length, out transferred, FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT | FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT);

				if (!Device.Ok(result))
					throw new TempSensorException("Could not setup Ta read");

				var crc = PEC.Calculate(Address, command, taData[0], taData[1]);
				//if (crc != taData[2])
				//	Console.WriteLine("PEC doesn't match");

				Console.WriteLine($"{command}: {taData[0]},{taData[1]}");

				return ((taData[1] << 8) | taData[0]) * 0.02;
			});
		}

		protected double ConvertKelvinToC(double kelvin) => kelvin - 273;
		protected double ConvertCtoF(double celcius) => (9 / 5.0 * celcius) + 32;
		protected double ConvertFtoC(double farenheit) => (farenheit - 32) * (5 / 9.0);

		protected Task WriteEeprom(byte address, byte[] values, FtI2CTransferOptions options = FtI2CTransferOptions.I2C_TRANSFER_OPTIONS_START_BIT)
		{
			// Sequence for writing a value:
			// 1. Power up device
			// 2. Write 0x0000 into the cell of interest
			// 3. Wait at least 5ms
			// 4. Write the new value
			// 5. Wait at least 5ms
			// 6. Read back and compare if the write was successful
			// 7. Power down (to make sure the changes take place at next power up

			return Task.Run(async () =>
			{
				var result = Device.Write(Address, new byte[] { address }.Concat(new byte[values.Length].Select(_ => (byte)0)).ToArray(), values.Length + 1, out int transfered, options);
				if (result != FtdiMpsseI2CResult.Ok)
					throw new TempSensorException("Could not write 0 into bus address");
				await Task.Delay(6);

				result = Device.Write(Address, new byte[] { address }.Concat(values).ToArray(), values.Length + 1, out transfered, options);
				if (!Device.Ok(result))
					throw new TempSensorException("Could not set new address");
				await Task.Delay(6);

				var readData = new byte[values.Length];
				result = Device.Write(Address, new byte[] { address }, values.Length, out transfered, options);
				result = Device.Read(Address, readData, values.Length, out transfered);
				if (!Device.Ok(result))
					throw new TempSensorException("Could not read I2C address");

				bool isValid = true;
				for (int i = 0; i < readData.Length; i++)
					isValid = isValid && readData[i] == values[0];

				if (!isValid)
					throw new TempSensorException("Temperature sensor address was not set successfully");

				Address = address;
			});
		}

		private bool DisposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!DisposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				DisposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Mlx90614()
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
	}
}


/* Config information
 *
 * The following is a description of the meaning of the bits of the config bytes.
 * |  Bit  | Description
 * |   15  | Enable/Disable sensor test
 * |   14  | Positive/Negative sign of K12
 * | 13-11 | Gain. Each higher number is from the following list: Bypassed, 3, 6, 12.5, 25, 50, 100, 100
 * | 10-8  | FIR. From the list starting with 0-7. 0-3 are not recommended. 8, 16, 32, 64, 128, 256, 512, 1024
 * |   7   | Positive sign of Ks
 * |   6   | 0 = single IR; 1 = Dual IR
 * |  5-4  | 0=Ta, Tobj1; 1=Ta, Tobj2; 2=Tobj2; 3=Tobj1, Tobj2
 * |   3   | Repeat sensor test. 0=OFF, 1=ON
 * |  2-0  | IIR. 4=100%, 5=80%, 6=67%, 7=57%, 0=50%, 1=25%, 2=17%, 3=13%
 *
 */
