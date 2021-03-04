﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing.Tools
{
	public static partial class Converter
	{
		public static short BytesToShort(byte[] bytes)
		{
			if (bytes.Length != 2)
				throw new ArgumentException($"Exactly 2 bytes requiered, got {bytes.Length} bytes.");
			short res = bytes[1];
			res = (short)(res << 8);
			res += bytes[0];
			return res;
		}

		public static int BytesToInt(byte[] bytes)
		{
			if (bytes.Length != 4)
				throw new ArgumentException($"Exactly 4 bytes requiered, got {bytes.Length} bytes.");
			int res = bytes[3];
			for (int i = 2; i >= 0; i--)
			{
				res = res << 8;
				res += bytes[i];
			}

			return res;
		}

		public static uint BytesToUInt(byte[] bytes)
		{
			uint res = bytes[bytes.Length - 1];
			for (int i = bytes.Length - 2; i >= 0; i--)
			{
				res = res << 8;
				res += bytes[i];
			}

			return res;
		}


		/// <summary>
		/// Transforms byte audio data into short audio data
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static short[] BytesToShorts(byte[] data)
		{
			short[] dataShorts = new short[data.Length / 2];

			for (int i = 0; i < data.Length; i += 2)
			{
				dataShorts[i / 2] = Tools.Converter.BytesToShort(new byte[] { data[i], data[i + 1] });
			}

			return dataShorts;
		}
	}
}
