using FTD2XX_NET;
using MadeInTheUSB.FT232H;
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
            LibMpsse.Init();

            CheckResult(LibMpsse.I2C_GetNumChannels(out int channels));

            for (int channel = 0; channel < channels; channel++)
            {
                CheckResult(LibMpsse.I2C_GetChannelInfo(channel, out FT_DEVICE_LIST_INFO_NODE info));
                WriteLine($"Channel {channel}:");
                WriteLine($"Flags: {info.Flags}; Type: {info.Type}");

                CheckResult(LibMpsse.I2C_OpenChannel(channel, out IntPtr device));

                I2C_CHANNEL_CONFIG config = new I2C_CHANNEL_CONFIG
                {
                    ClockRate = I2C_CLOCKRATE.I2C_CLOCK_FAST_MODE,
                    LatencyTimer = 255
                };

                CheckResult(LibMpsse.I2C_InitChannel(device, ref config));

                // Read & Write To/From Channel
                byte[] buffer = new byte[1];
                int sizeTransferred;
                try
                {
                    CheckResult(LibMpsse.I2C_DeviceRead(device, 0x40, 1, buffer, out sizeTransferred, 0));
                } 
                catch (InvalidOperationException ex)
                {
                    WriteLine(ex.ToString());
                }

                try
                {
                    CheckResult(LibMpsse.I2C_DeviceWrite(device, 0x40, 1, new byte[] { 0xA5 }, out sizeTransferred, 0));
                }
                catch (InvalidOperationException ex)
                {
                    WriteLine(ex.ToString());
                }

                CheckResult(LibMpsse.I2C_CloseChannel(device));
            }

            LibMpsse.Cleanup();
        }

        private static void CheckResult(FTDI.FT_STATUS fT_STATUS)
        {
            if (fT_STATUS != FTDI.FT_STATUS.FT_OK)
                throw new InvalidOperationException("Result from function call was: " + fT_STATUS.ToString());
        }
    }
}
