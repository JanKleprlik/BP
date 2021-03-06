﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AudioRecognitionLibrary.Tools
{
	internal static class Arithmetics
	{
		/// <summary>
		/// Gets average of two values
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>Average value of given values.</returns>
		public static short Average(short left, short right)
		{
			return (short)((left + right) / 2);
		}

		/// <summary>
		/// Swap two values
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first">Value 1</param>
		/// <param name="second">Value 2</param>
		/// <returns>Value 2</returns>
		public static T Swap<T>(this T first, ref T second)
		{
			T tmp = second;
			second = first;
			return tmp;
		}

		/// <summary>
		/// Returns <c>_base</c> to the power of <c>exp</c>
		/// </summary>
		/// <param name="_base"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public static int ToPowOf(this int _base, int exp)
		{
			int res = _base;
			for (int i = 0; i < exp; i++)
			{
				res *= _base;
			}

			return res;
		}

		/// <summary>
		/// Returns second power of <c>_base</c>
		/// </summary>
		/// <param name="_base"></param>
		/// <returns></returns>
		public static double Pow2(this double _base)
		{
			return _base * _base;
		}
		/// <summary>
		/// Get absolute value of complex number.
		/// </summary>
		/// <param name="real">Real part of the complex number.</param>
		/// <param name="img">Imaginary part of the complex number.</param>
		/// <returns>Absolute value of the complex number.</returns>
		public static double GetComplexAbs(double real, double img)
		{
			return Math.Sqrt(real.Pow2() + img.Pow2());
		}
		/// <summary>
		/// Determines if n is a power of two.
		/// </summary>
		/// <param name="n"></param>
		/// <returns>True if it is power of two, false otherwise.</returns>
		public static bool IsPowOfTwo(int n)
		{
			if ((n & (n - 1)) != 0)
				return false;
			return true;
		}
	}

	internal static class Builders
	{
		/// <summary>
		/// Builds address from parts
		/// </summary>
		/// <param name="anchorFreq">Frequency of anchor point</param>
		/// <param name="pointFreq">Frequency of Self point</param>
		/// <param name="delta">Time delta between Anchor and Self point</param>
		/// <returns>Left to right: 9bits Anchor frequency, 9bits Self point frequency, 14 bits delta</returns>
		internal static uint BuildAddress(in uint anchorFreq, in uint pointFreq, uint delta)
		{
			uint res = anchorFreq;
			res <<= 9; //move 9 bits 
			res += pointFreq;
			res <<= 14; //move 14 bits 
			res += delta;
			return res;
		}

		/// <summary>
		/// Builds song value out of parts
		/// </summary>
		/// <param name="absAnchorTime">Absolute time of anchor</param>
		/// <param name="id">ID of a song</param>
		/// <returns>Left to right: 32bits AbsAnchTime, 32 bits songID</returns>
		internal static ulong BuildSongValue(in uint absAnchorTime, uint id)
		{
			ulong res = absAnchorTime;
			res <<= 32;
			res += id;
			return res;
		}
	}
}
