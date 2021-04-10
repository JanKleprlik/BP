﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BP.Shared.Models
{
	public class Settings
	{
		public Settings()
		{
			SetToDefault();
		}

		/// <summary>
		/// <b>True</b>: Use Constant Q algorithm to get frequency domain.<br/>
		/// <b>False</b>: Use FFT to get frequency domain.<br/>
		/// Default: False
		/// </summary>
		public bool ConstQAlgorithm { get; set; }

		/// <summary>
		/// <b>True</b>: Print detailed information about recognition. <br/>
		/// <b>False</b>: Print only result.<br/>
		/// Default: False
		/// </summary>
		public bool DetailedInfo { get; set; }

		/// <summary>
		/// ONLY WASM <br/>
		/// <b>True</b>: Use microphone for recording.<br/>
		/// <b>False</b>: Use file upload for recording.<br/>
		/// Default: False
		/// </summary>
		public bool UseMicrophone { get; set; }

		/// <summary>
		/// The time for which the recording will be recorded.<br/>
		/// Minimum 3 sec <br/>
		/// Maximum 8 sec <br/>
		/// Default: 3 sec
		/// </summary>
		public int RecordingLength { 
			get	{
				return this.recordingLength;
			}
			set
			{
				this.recordingLength = Math.Max(Math.Min(value, 8), 3); //set min 3, max 10 secs
			}
		}
		/// <summary>
		/// Backing field
		/// </summary>
		private int recordingLength;

		public void SetToDefault()
		{
			ConstQAlgorithm = false;
			DetailedInfo = false;
			UseMicrophone = false;
			RecordingLength = 3;
		}

		public override string ToString()
		{
			string text = "SETTINGS: \n" +
				$"ConstQAlgorithm: {ConstQAlgorithm}\n" +
				$"DetailedInfo: {DetailedInfo}\n" +
				$"UseMicrophone: {UseMicrophone}\n" +
				$"RecordingLength: {RecordingLength}";
			return text;
		}
	}
}