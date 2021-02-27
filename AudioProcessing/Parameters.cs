﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
	public partial class AudioRecognizer
	{
		static class Parameters
		{
			public const int TargetSamplingRate = 12000;

			/// <summary>
			/// Default size of FFT window
			/// </summary>
			public static int WindowSize = 4096;
			/// <summary>
			/// Default downsample coeficient
			/// </summary>
			public static int DownSampleCoef = 4;
			/// <summary>
			/// Default size of target zone
			/// </summary>
			public static int TargetZoneSize = 5;
			/// <summary>
			/// Default offset of anchor from first actual point
			/// </summary>
			public static int AnchorOffset = 2;
			/// <summary>
			/// Obligated portion of samples in TGZ
			/// </summary>
			public static double SamplesInTgzCoef = 0.55;
			/// <summary>
			/// Obligated portion of time coherent notes
			/// </summary>
			public static double CoherentNotesCoef = 0.4;

		}
	}
}