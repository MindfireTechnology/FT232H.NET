# MadeInTheUSB FT232H NET Library

A .NET/Windows library to talk to the FTDI chip FT232H.

The library MadeInTheUSB.FT232H provides an abstraction to program 
* The SPI protocol
* The GPIOs
for break out based on the FTDI chip ft232H.

Not supported yet:
- I2C
	* With the FT232H it is not possible to used I2C and SPI at the same time, because the 2 protocols
	use the same clock and data pins.
	* To support I2C and SPI at the same time you can use the FT2232H or the FT4222H.

- Serial communication
 
## Breakouts available
 
 * The Adafruit breakout: 
 [Adafruit FT232H Breakout](https://www.adafruit.com/product/2264) 
 	- General Purpose USB to GPIO+SPI+I2C

 * [Chinese/eBay FT232H breakout](https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2380057.m570.l1313.TR12.TRC2.A0.H0.Xft232H.TRS0&_nkw=ft232H&_sacat=0)
 	- This breakout does not contains an EEPROM therefore it is possible to program the device id or description.
	- SPI and GPIOs are working fine.
 
 * [Chinese/eBay FT 2 232H breakout](https://www.ebay.com/itm/1-pcs-USB-to-UART-FIFO-SPI-I2C-JTAG-RS232-module-FT2232HL-D1L2/253767822756?epid=14010988565&hash=item3b15bdada4)
	- I never tested this breakout, but seems interesting

 ## References Links

- [FT232H Datasheet](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT232H.pdf)
- [LibMPSSE](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI.htm)
- [LibMPSSE-SPI library and examples](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/LibMPSSE-SPI.zip)
- [LibMPSSE - Release notes](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/ReleaseNotes-SPI.txt)

- [FTDI Program Guides](https://www.ftdichip.com/Support/Documents/ProgramGuides.htm)
- [Application Note AN_177 User Guide For libMPSSE – I2C](https://www.ftdichip.com/Support/Documents/AppNotes/AN_177_User_Guide_For_LibMPSSE-I2C.pdf)


- [Speaking SPI & I2C With The FT-2232](http://www.devttys0.com/2011/11/speaking-spi-i2c-with-the-ft-2232/)

## SPI, I2C, GPIO Wiring

 * SPI
	- Clock AD0
	- MOSI  AD1
	- MISO  AD2
	- CS0   AD3, CS1:AD4, CS2:AD5, CS3:AD6, CS4:AD7

 * I2C
	- Clock   AD0
	- SDA OUT AD1
	- SDA IN  AD2

	- SDA OUT and SDA IN need to be connected because in I2C there is only one data write.
	- The data and clock wire each requires a pull up resistor.
	aaa

 * GPIOS
	- GPIO 0..7: AC0..C7. 
	- AC8, AC9 are special and not supported yet by the lirbary
	
 ## .NET Compilation

 * x64 : This code must be compiled in 64 bit mode

 ## Dll dependency and drivers

* The dll FTD2XX.DLL, must be in the path. The dll should be installed by the FTDI driver.
 The driver should automatically be installed by Windows 10 on the first time the FT232H or FT232RL is connected
  to the machine. For Windows 7 install the driver manually.

* This library contains the source code of the .NET wrapper for the dll FTD2XX.DLL.
The file is called FTD2XX_NET.cs. This is the last version from FTDT as 2018, that support the FT4222H.

* The dll 
[libMPSSE.dll ](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/LibMPSSE-SPI.zip)
from FTDT must be in the current folder. It is part of the source code.