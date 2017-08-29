using System;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using MP3Sharp;

namespace Terraria.ModLoader.Audio
{
	public abstract class Music
	{
		public static implicit operator Music(Cue cue) { return new MusicCue() { cue = cue }; }
		public abstract bool IsDisposed { get; }
		public abstract bool IsPaused { get; }
		public abstract bool IsPlaying { get; }
		public abstract void Pause();
		public abstract void Play();
		public abstract void Resume();
		public abstract void Stop(AudioStopOptions options);
		public abstract void SetVariable(string name, float value);
		public virtual void CheckBuffer() { }
	}

	public class MusicCue : Music
	{
		internal Cue cue;
		//public static implicit operator Cue(MusicCue musicCue){return musicCue.cue;}
		public override bool IsDisposed { get { return cue.IsDisposed; } }
		public override bool IsPaused { get { return cue.IsPaused; } }
		public override bool IsPlaying { get { return cue.IsPlaying; } }
		public override void Pause() { cue.Pause(); }
		public override void Play() { cue.Play(); }
		public override void Resume() { cue.Resume(); }
		public override void Stop(AudioStopOptions options) { cue.Stop(options); }
		public override void SetVariable(string name, float value) { cue.SetVariable(name, value); }
	}

	public abstract class MusicStreaming : Music, IDisposable
	{
		private const int bufferMin = 3;
		private const int bufferCountPerSubmit = 3;
		internal const int DEFAULT_BYTESPERCHUNK = 4096;
		internal static byte[] buffer = new byte[DEFAULT_BYTESPERCHUNK];
		internal DynamicSoundEffectInstance instance;
		internal Stream stream;
		internal long dataStart;

		public override bool IsDisposed => instance.IsDisposed;
		public override bool IsPaused => instance.State == SoundState.Paused;
		public override bool IsPlaying => instance.State != SoundState.Stopped;
		public override void Pause() => instance.Pause();
		public override void Play() => instance.Play();
		public override void Resume() => instance.Resume();
		public override void SetVariable(string name, float value)
		{
			switch (name)
			{
				case "Volume": instance.Volume = value; return;
				case "Pitch": instance.Pitch = value; return;
				case "Pan": instance.Pan = value; return;
				default: throw new Exception("Invalid field: '" + name + "'");
			}
		}
		public override void Stop(AudioStopOptions options) => instance.Stop();

		public override void CheckBuffer()
		{
			if (instance.PendingBufferCount < bufferMin && IsPlaying)
			{
				SubmitBuffer(bufferCountPerSubmit);
			}
		}

		internal void SubmitBuffer(int count)
		{
			if (stream == null)
			{
				instance.Stop();
				return;
			}
			for (int i = 0; i < count; i++)
			{
				if (!SubmitSingle())
				{
					if (i == 0)
					{
						instance.Stop();
					}
					break;
				}
			}
		}

		private bool SubmitSingle()
		{
			//byte[] buffer = new byte[DEFAULT_BYTESPERCHUNK];
			int bytesReturned = stream.Read(buffer, 0, buffer.Length);
			if (bytesReturned < DEFAULT_BYTESPERCHUNK)
			{
				ResetStreamPosition();
				stream.Read(buffer, bytesReturned, buffer.Length - bytesReturned);
			}
			instance.SubmitBuffer(buffer);
			return true;
		}

		public void ResetStreamPosition()
		{
			if (stream != null)
			{
				stream.Position = 0;
				MP3Stream mp3 = stream as MP3Stream;
				if (mp3 != null)
					mp3.IsEOF = false;
			}
		}

		public void Dispose()
		{
			if (instance.State == SoundState.Playing)
			{
				instance.Stop();
			}

			instance.Dispose();
			instance = null;

			stream.Close();
			stream = null;
		}
	}

	public class MusicStreamingWAV : MusicStreaming
	{
		public MusicStreamingWAV(string path)
		{
			stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			Setup();
		}

		public MusicStreamingWAV(Stream stream)
		{
			this.stream = stream;
			Setup();
		}

		// Parses the header and sets dataStart and instance
		private void Setup()
		{
			BinaryReader reader = new BinaryReader(stream);
			int chunkID = reader.ReadInt32();
			int fileSize = reader.ReadInt32();
			int riffType = reader.ReadInt32();
			int fmtID = reader.ReadInt32();
			int fmtSize = reader.ReadInt32();
			int fmtCode = reader.ReadInt16();
			int channels = reader.ReadInt16();
			int sampleRate = reader.ReadInt32();
			int fmtAvgBPS = reader.ReadInt32();
			int fmtBlockAlign = reader.ReadInt16();
			int bitDepth = reader.ReadInt16();

			if (fmtSize == 18)
			{
				// Read any extra values
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}

			int dataID = reader.ReadInt32();
			int dataSize = reader.ReadInt32();

			dataStart = reader.BaseStream.Position;
			stream.Position = dataStart;

			instance = new DynamicSoundEffectInstance(sampleRate, (AudioChannels)channels);
		}
	}

	public class MusicStreamingMP3 : MusicStreaming
	{
		public MusicStreamingMP3(byte[] data)
		{
			MP3Stream stream = new MP3Stream(new MemoryStream(data));
			instance = new DynamicSoundEffectInstance(stream.Frequency, (AudioChannels)stream.ChannelCount);
			this.stream = stream;
		}
	}

	// byte array pointing to mp3 data or wav data
	public class MusicData
	{
		string cachePath;
		byte[] data;
		bool mp3 = false;

		public MusicData(string cachePath)
		{
			this.cachePath = cachePath;
		}

		public MusicData(byte[] data, bool mp3)
		{
			this.data = data;
			this.mp3 = mp3;
		}

		public Music GetInstance()
		{
			if (cachePath != null)
			{
				if (mp3)
					throw new Exception("Cache and MP3 not implemented");
				else
					return new MusicStreamingWAV(cachePath);
			}
			else if (data != null)
			{
				if (mp3)
					return new MusicStreamingMP3(data);
				else
					return new MusicStreamingWAV(new MemoryStream(data));
			}
			throw new Exception("Error, MusicWrapper neither cache nor data supplied.");
		}
	}
}
