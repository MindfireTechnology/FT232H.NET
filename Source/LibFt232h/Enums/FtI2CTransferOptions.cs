using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    [Flags]
    public enum FtI2CTransferOptions : int
    {
        None              = 0x00000000,
        /// <summary>Generate start condition before transmitting</summary>
        I2C_TRANSFER_OPTIONS_START_BIT = 0b1,
        /// <summary>Generate stop condition before transmitting</summary>
        I2C_TRANSFER_OPTIONS_STOP_BIT = 0b10,

        /// <summary>libMPSSE-I2C generates an ACKs for every byte read. Some I2C slaves require the I2C
        /// master to generate a nACK for the last data byte read. Setting this bit enables working with such
        /// I2C slaves</summary>
        I2C_TRANSFER_OPTIONS_BREAK_ON_NACK = 0b100,

        /// <summary>no address phase, no USB interframe delays</summary>
        I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE = 0b1000,
        /// <summary></summary>
        I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES = 0b10000,
        /// <summary></summary>
        I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS = 0x10,
        /// <summary></summary>
        I2C_TRANSFER_OPTIONS_FAST_TRANSFER = 0x20,


        /// <summary>if I2C_TRANSFER_OPTION_FAST_TRANSFER is set then setting this bit would mean that the
        /// address field should be ignored. The address is either a part of the data or this is a special I2C
        /// frame that doesn't require an address</summary>
        I2C_TRANSFER_OPTIONS_NO_ADDRESS = 0x40,
    }
}
