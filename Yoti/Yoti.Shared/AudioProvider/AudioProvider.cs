﻿// AudioDataProvider is for UWP and ANDROID only
// so we hide implementation from WASM to avoid compilation errors.
#if NETFX_CORE || __ANDROID__
using System;
using System.Threading.Tasks;


#if NETFX_CORE
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

#endif

#if __ANDROID__
using Android.Media;
using System.IO;
using System.Threading;
using Xamarin.Essentials;
using System.Collections.Generic;
using Windows.UI.Core;
#endif

namespace Yoti.Shared.AudioProvider
{
	/// <summary>
	/// Adapter for obtaining audio both by recording with microphone and by picking file with FilePicker.<br></br>
	/// UWP and ANDROID only.
	/// </summary>
	public sealed partial class AudioDataProvider
	{


		#region private properties
		/// <summary>
		/// Lock used for audio buffers.
		/// </summary>
		private object bufferLock { get; set; } = new object();
		private bool isRecording { get; set; }

		// UWP specific properties
		#region UWP
#if NETFX_CORE
		/// <summary>
		/// Media for audio capture via micorphone.
		/// </summary>
		private MediaCapture audioCapture { get; set; }
		/// <summary>
		/// Audio buffer.
		/// </summary>
		private IRandomAccessStream buffer { get; set; }
		/// <summary>
		/// Default name of tmp file to store recorded audio into so it can be replayed.
		/// </summary>
		private string audioFile { get; set; } = "recording.wav";
#endif
		#endregion

		// ANDROID specific properties
		#region ANDROID
#if __ANDROID__
		/// <summary>
		/// Audio buffer.
		/// </summary>
		private byte[] buffer { get; set; }
		/// <summary>
		/// Audio Recorder
		/// </summary>
		private AudioRecord recorder { get; set; }
		/// <summary>
		/// Audio Buffer size limit.
		/// Default value 441 000 is 5 seconds of recording.
		/// </summary>
		private int bufferLimit { get; set; } = 441000;
#endif
		#endregion

		#endregion

		/// <summary>
		/// Open platform specific FilePicker and allow user to pick audio file of supported type.
		/// </summary>
		/// <param name="writeResult">Action accepting result of Upload process described in string.</param>
		/// <returns>True if picked song was sucessfuly uploaded, false otherwise.</returns>
		public async Task<bool> UploadRecording(Action<string> writeResult, ulong maxSize_MB)
		{
			#region UWP
#if NETFX_CORE
			// Setup FilePicker
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
			picker.FileTypeFilter.Add(".wav");

			// Open FilePicker
			StorageFile file = await picker.PickSingleFileAsync();

			if (file == null)
			{
				writeResult("No song uploaded.");
				return false;
			}

			// Check size of uploaded file
			var fileProperties = await file.GetBasicPropertiesAsync();
			if (fileProperties.Size > maxSize_MB * 1024 * 1024)
			{
				writeResult($"Selected file is too big." + Environment.NewLine + $"Maximum allowed size is {maxSize_MB} MB.");
				return false;
			}
			else
			{
				buffer = await file.OpenAsync(FileAccessMode.ReadWrite);
				writeResult(file.Name);
				return true;
			}
#endif
			#endregion

			#region ANDROID
#if __ANDROID__
			if (await Utils.Permissions.Droid.GetExternalStoragePermission())
			{
				// Setup FilePicker
				PickOptions options = new PickOptions
				{
					PickerTitle = "Please select a wav song file",
					FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
					{
						{DevicePlatform.Android, new[]{"audio/x-wav", "audio/wav"} }
					})
				};

				// Open FilePicker
				FileResult result = await FilePicker.PickAsync(options);

				// Process picked file
				if (result != null)
				{
					var audioFileData = await result.OpenReadAsync();
					if ((ulong)audioFileData.Length > maxSize_MB * 1024 * 1024)
					{
						writeResult($"File is too large." + Environment.NewLine + $"Maximum allowed size is {maxSize_MB} MB.");
						return false;
					}

					buffer = new byte[(int)audioFileData.Length];
					audioFileData.Read(buffer, 0, (int)audioFileData.Length);
					writeResult(result.FileName);

					return true;
				}
				// No file picked
				else
				{
					writeResult("No song uploaded");
					return false;
				}

			}
			// External Sotrage Permission not granted
			else
			{
				writeResult("Acces to read storage denied.");
				return false;
			}
#endif
			#endregion
			// Fallback value for unsupported platforms
			return false;
		}

		/// <summary>
		/// Platform specific audio recording using microphone.
		/// </summary>
		/// <param name="recordingLength"></param>
		/// <returns>True if recording was sucessful, false otherwise.</returns>
		public async Task<bool> RecordAudio(int recordingLength = 5)
		{
#if __ANDROID__
			// Make sure we have Microphone permission on ANDROID devices
			if (!await Utils.Permissions.Droid.GetMicPermission())
			{
				System.Diagnostics.Debug.WriteLine("[DEBUG] microphone acecss denied.");
				return false;
			}

			bufferLimit = GetBufferLimitFromTime(recordingLength);
#endif

			await StartRecording();

#if NETFX_CORE
			// Record audio for recordingLength seconds
			await Task.Delay(recordingLength * 1000);
#endif

			await StopRecording();
			return true;
		}

		/// <summary>
		/// Returns raw audio data.
		/// </summary>
		/// <returns>Raw audio data as an array of bytes.</returns>
		public async Task<byte[]> GetDataFromStream()
		{
			#region UWP
#if NETFX_CORE
			if (buffer == null)
				throw new InvalidOperationException("Buffer should not be null");

			byte[] data = new byte[buffer.Size];
			DataReader dataReader = new DataReader(buffer.GetInputStreamAt(0));

			await dataReader.LoadAsync((uint)buffer.Size);
			dataReader.ReadBytes(data);
			return data;
#endif
			#endregion

			#region ANDROID
#if __ANDROID__
			return buffer;
#endif
			#endregion

		}

		/// <summary>
		/// Replays recorded audio by microphone.
		/// </summary>
		/// <param name="UIDispatcher">UIDispatcher needed for UWP platform.</param>
		public async void ReplayRecording(CoreDispatcher UIDispatcher)
		{
			#region UWP
#if NETFX_CORE
			// Do nothign without buffer
			if (buffer == null)
				return;


			MediaElement playback = new MediaElement();
			IRandomAccessStream audioBuffer;

			lock (bufferLock)
			{
				audioBuffer = buffer.CloneStream();
			}

			if (audioBuffer == null)
				throw new ArgumentNullException("buffer");

			// Replay async
			await UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				playback.SetSource(audioBuffer, "");
				playback.Play();
			});
#endif
			#endregion
			#region ANDROID
#if __ANDROID__
			// Setup AudioTrack
			ChannelOut channels = Parameters.Channels == 1 ? ChannelOut.Mono : ChannelOut.Stereo;
			AudioTrack audioTrack = new AudioTrack.Builder()
				.SetAudioAttributes(new AudioAttributes.Builder()
					.SetUsage(AudioUsageKind.Media)
					.SetContentType(AudioContentType.Music)
					.Build())
				.SetAudioFormat(new AudioFormat.Builder()
					.SetEncoding(Encoding.Pcm16bit)
					.SetSampleRate((int)Parameters.SamplingRate)
					.SetChannelMask(channels)
					.Build())
				.SetBufferSizeInBytes(buffer.Length)
				.Build();


			int totalBytesReplayed = 0;
			// Replay audio in lock until whole buffer is replayed
			lock (bufferLock)
			{
				audioTrack.Play();

				while (totalBytesReplayed < bufferLimit)
				{
					try
					{
						totalBytesReplayed = audioTrack.Write(buffer, 0, bufferLimit);
						System.Diagnostics.Debug.WriteLine($"Read: {totalBytesReplayed}");
						if (totalBytesReplayed < 0)
							throw new Exception(String.Format("Exception code: {0}", totalBytesReplayed));
						
					}
					catch (Exception e)
					{
						// Invalidate audio buffer
						buffer = null;
						break;
					}
				}
			}
#endif
			#endregion

		}

		#region Helper methods
		/// <summary>
		/// Starts the recording process.
		/// </summary>
		private async Task StartRecording()
		{
			if (isRecording)
			{
				// Already recording
				return;
			}
			else
			{
				#region UWP
#if NETFX_CORE
				// Prepare MediaRecorder and encoding profile
				await SetupRecording();
				var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);
				profile.Audio = AudioEncodingProperties.CreatePcm((uint)Parameters.SamplingRate, (uint)Parameters.Channels, 16);

				// Start recording
				isRecording = true;
				await audioCapture.StartRecordToStreamAsync(profile, buffer);
				return;
#endif
				#endregion
				#region ANDROID
#if __ANDROID__

				// Setup recorder
				ChannelIn channels = Parameters.Channels == 1 ? ChannelIn.Mono : ChannelIn.Stereo;
				recorder = new AudioRecord(
					AudioSource.Mic,
					(int)Parameters.SamplingRate,
					channels,
					Android.Media.Encoding.Pcm16bit,
					bufferLimit
					);
			

				// Start recording
				isRecording = true;
				int totalBytesRead = 0;
				buffer = new byte[bufferLimit];

				// lock buffer so that no other thread can acces the buffer (ie. replay while recording)
				// and create inconsistent audio data
				lock (bufferLock)
				{
					recorder.StartRecording();
					// Record audio until buffer is full
					while (totalBytesRead < bufferLimit)
					{
						try
						{
							totalBytesRead = recorder.Read(buffer, 0, bufferLimit);
							if (totalBytesRead < 0)
								throw new Exception(String.Format("Exception code: {0}", totalBytesRead));
						}
						catch (Exception e)
						{
							// Invalidate audio buffer
							buffer = null;
							break;
						}
					}
				
				}
#endif
				#endregion
			}
		}

		/// <summary>
		/// Stops the recording process.
		/// </summary>
		private async Task StopRecording()
		{
			if (!isRecording)
			{
				//not recording yet
				return;
			}
			else
			{
				#region UWP
#if NETFX_CORE
				await audioCapture.StopRecordAsync();
#endif
				#endregion
				#region ANDROID
#if __ANDROID__
				recorder.Stop();
				recorder.Dispose();
#endif
				#endregion
				isRecording = false;
			}
		}


		#region UWP
#if NETFX_CORE

		/// <summary>
		/// Sets audioCapture to proper settings so we get 16 bit PCM audio data at 
		/// desired sample rate and number of audio channels.
		/// </summary>
		/// <returns></returns>
		private async Task SetupRecording()
		{
			// Reset resources
			if (buffer != null)
				buffer.Dispose();
			if (audioCapture != null)
				audioCapture.Dispose();
			buffer = new InMemoryRandomAccessStream();


			try
			{

				// Initialization
				audioCapture = new MediaCapture();
				await audioCapture.InitializeAsync(new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio });
				
				// Set exceed limitation handler
				audioCapture.RecordLimitationExceeded += async (MediaCapture sender) =>
				{
					await audioCapture.StopRecordAsync();
					isRecording = false;
					throw new Exception("Record limitation exceeded.");
				};

				// On fail handler
				audioCapture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) =>
				{
					isRecording = false;
					throw new Exception(string.Format("Audio Capture failed. CODE: {0}. {1}", errorEventArgs.Code, errorEventArgs.Message));
				};
			}
			catch(Exception e)
			{
				//Propagate the exception
				throw e;
			}
		}

#endif
		#endregion

		#region ANDROID
#if __ANDROID__
		/// <summary>
		/// Computes buffer length needed to capture recordingLength seconds of audio.
		/// </summary>
		/// <param name="recordingLength"></param>
		/// <returns>Desired size of buffer</returns>
		private int GetBufferLimitFromTime(int recordingLength)
		{
			//seconds * (2 bytes per 1 sample) * samplingRate
			return recordingLength * 2 * (int)Parameters.SamplingRate;
		}
#endif
		#endregion
		#endregion

	}
}
#endif
