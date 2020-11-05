using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static void GpioSample(IDigitalWriteRead gpios, bool oneLoopOnly = false)
        {
            var goOn           = true;
            const int waitTime = 65;

            while(goOn) {

                for(var i=0; i < gpios.MaxGpio; i++)
                {
                    gpios.DigitalWrite(i, PinState.High);
                    Thread.Sleep(waitTime);
                }
                Thread.Sleep(waitTime);
                for(var i=0; i < gpios.MaxGpio; i++)
                {
                    gpios.DigitalWrite(i, PinState.Low);
                    Thread.Sleep(waitTime);
                }
                Thread.Sleep(waitTime);
                if(System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey();
                    if(k.Key == ConsoleKey.Q)
                        goOn = false;
                }
                if(oneLoopOnly)
                    goOn = false;
            }
        }
    }
}
