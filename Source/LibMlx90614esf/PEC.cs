using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMlx90614esf
{
	public class PEC
	{
		public static byte Calculate(IEnumerable<byte> bytes) => Calculate(bytes.ToArray());

		public static byte Calculate(params byte[] bytes)
		{
			byte pec = 0;

			foreach (var b in bytes)
			{
				pec ^= b;
				pec = (byte)(pec ^ (pec << 1) ^ (pec << 2) ^ (((pec & 128) != 0) ? 9 : 0) ^ (((pec & 64) != 0) ? 7 : 0));
			}

			return pec;
		}
	}
}
