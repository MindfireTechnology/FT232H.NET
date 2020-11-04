/*
** FTD2XX_NET.cs
**
** Copyright © 2009-2013 Future Technology Devices International Limited
**
** C# Source file for .NET wrapper of the Windows FTD2XX.dll API calls.
** Main module
**
** Author: FTDI
** Project: CDM Windows Driver Package
** Module: FTD2XX_NET Managed Wrapper
** Requires: 
** Comments:
**
** History:
**  1.0.0	-	Initial version
**  1.0.12	-	Included support for the FT232H device.
**  1.0.14	-	Included Support for the X-Series of devices.
**  1.0.16  -	Overloaded constructor to allow a path to the driver to be passed.
**  1.1.0   -   Handle full 16 character Serial Number and support FT4222 programming board.
** 
*/


namespace FTD2XX_NET
{
	public interface IFTDI
	{
		bool IsOpen { get; }

		FTDI.FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead);
		FTDI.FT_STATUS Write(byte[] dataBuffer, int numBytesToWrite, ref uint numBytesWritten);
		FTDI.FT_STATUS Write(byte[] dataBuffer, uint numBytesToWrite, ref uint numBytesWritten);
		FTDI.FT_STATUS Write(string dataBuffer, int numBytesToWrite, ref uint numBytesWritten);
		FTDI.FT_STATUS Write(string dataBuffer, uint numBytesToWrite, ref uint numBytesWritten);
	}
}