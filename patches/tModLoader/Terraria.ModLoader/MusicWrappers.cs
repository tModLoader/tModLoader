using System;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using MP3Sharp;
using NVorbis;

//TODO refactor to Terraria.ModLoader, delayed due to breaking change (public in Mod[Content].GetMusic)
namespace Terraria.ModLoader.Audio
{
	public abstract class Music
	{
		public static implicit operator Music(Cue cue) { return new MusicCue() { cue = cue }; }
		public abstract bool IsPaused { get; }
		public abstract bool IsPlaying { get; }
		public abstract void Reset();
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
		public override bool IsPaused => cue.IsPaused;
		public override bool IsPlaying => cue.IsPlaying;
		public override void Pause() => cue.Pause();
		public override void Play() => cue.Play();
		public override void Resume() => cue.Resume();
		public override void Stop(AudioStopOptions options) => cue.Stop(options);
		public override void SetVariable(string name, float value) => cue.SetVariable(name, value);
		
		public override void Reset() {
			cue = Main.soundBank.GetCue(cue.Name);
		}
	}

	public abstract class MusicStreaming : Music, IDisposable
	{
		// to play a 44.1kHz dual channel signal, we'd need to submit 735 samples per frame
		// at 4 bytes per sample = 2940bytes.
		// increasing the buffer size to 4096 and submitting 2 buffers at a time means we
		// need to submit 21.5 times per second, or roughly every 3 frames at 60fps
		private const int bufferLength = 4096;
		private const int bufferCountPerSubmit = 2;

		// with a 4 buffer minimum, we have roughly 1/10th of a second of stutter before the music drops
		private const int bufferMin = 4;
		
		private string path;

		private DynamicSoundEffectInstance instance;
		protected Stream stream;
		private byte[] buffer;
		
		protected int sampleRate;
		protected AudioChannels channels;

		public MusicStreaming(string path) {
			this.path = path;
		}
		
		public override bool IsPaused => instance != null && instance.State == SoundState.Paused;
		public override bool IsPlaying => instance != null && instance.State != SoundState.Stopped;
		public override void Pause() => instance.Pause();
		public override void Resume() => instance.Resume();
		public override void Play() {
			EnsureLoaded();
			instance.Play();
		}

		public override void SetVariable(string name, float value) {
			switch (name) {
				case "Volume": instance.Volume = value; return;
				case "Pitch": instance.Pitch = value; return;
				case "Pan": instance.Pan = value; return;
				default: throw new Exception("Invalid field: '" + name + "'");
			}
		}

		private void EnsureLoaded() {
			if (instance != null)
				return;
			
			stream = ModContent.OpenRead(path, true);
			PrepareStream();

			instance = new DynamicSoundEffectInstance(sampleRate, channels);
			buffer = new byte[bufferLength]; // could use a buffer pool but swapping music isn't likely to thrash the GC too much

			CheckBuffer();
		}

		protected abstract void PrepareStream();

		public override void Stop(AudioStopOptions options) {
			instance.Stop();

			instance.Dispose();
			instance = null;

			stream.Dispose();
			stream = null;

			buffer = null;
		}

		public override void CheckBuffer() {
			if (!IsPlaying || instance.PendingBufferCount >= bufferMin)
				return;

			for (int i = 0; i < bufferCountPerSubmit; i++)
				SubmitSingle();
		}

		private void SubmitSingle() {
			FillBuffer(buffer);
			instance.SubmitBuffer(buffer);
		}

		protected virtual void FillBuffer(byte[] buffer) {
			int read = stream.Read(buffer, 0, buffer.Length);
			if (read < buffer.Length) {
				Reset();
				stream.Read(buffer, read, buffer.Length - read);
			}
		}

		public void Dispose() {
			if (instance != null)
				Stop(AudioStopOptions.Immediate);
		}
	}

	public class MusicStreamingWAV : MusicStreaming
	{
		private long dataStart = -1;

		public MusicStreamingWAV(string path) : base(path) {}
		
		protected override void PrepareStream() {
			if (dataStart >= 0) {
				stream.Position = dataStart;
				return;
			}

			var reader = new BinaryReader(stream);
			int chunkID = reader.ReadInt32();
			int fileSize = reader.ReadInt32();
			int riffType = reader.ReadInt32();
			int fmtID = reader.ReadInt32();
			int fmtSize = reader.ReadInt32();
			int fmtCode = reader.ReadInt16();
			channels = (AudioChannels)reader.ReadInt16();
			sampleRate = reader.ReadInt32();
			int fmtAvgBPS = reader.ReadInt32();
			int fmtBlockAlign = reader.ReadInt16();
			int bitDepth = reader.ReadInt16();

			if (fmtSize == 18) {
				// Read any extra values
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}

			int dataID = reader.ReadInt32();
			int dataSize = reader.ReadInt32();
			dataStart = stream.Position;
		}

		public override void Reset() {
			if (stream != null)
				stream.Position = dataStart;
		}
	}

	public class MusicStreamingMP3 : MusicStreaming
	{
		private Stream underlying;

		public MusicStreamingMP3(string path) : base(path) {}

		protected override void PrepareStream() {
			underlying = stream;

			var mp3Stream = new MP3Stream(stream);
			sampleRate = mp3Stream.Frequency;
			channels = (AudioChannels)mp3Stream.ChannelCount;
			stream = mp3Stream;
		}

		public override void Stop(AudioStopOptions options) {
			base.Stop(options);
			underlying = null;
		}

		public override void Reset() {
			if (stream != null) {
				underlying.Position = 0;
				//mp3 is not designed to loop and creates static if you just reset the stream due to fourier encoding carryover
				//if you're really smart, you can make a looping version and PR it
				stream = new MP3Stream(underlying);
			}
		}
	}
	
	public class MusicStreamingOGG : MusicStreaming
	{
		private VorbisReader reader;
		private float[] floatBuf;

		public MusicStreamingOGG(string path) : base(path) {}

		protected override void PrepareStream() {
			reader = new VorbisReader(stream, true);
			sampleRate = reader.SampleRate;
			channels = (AudioChannels)reader.Channels;
		}

		protected override void FillBuffer(byte[] buffer) {
			if (floatBuf == null)
				floatBuf = new float[buffer.Length/2];

			int read = reader.ReadSamples(floatBuf, 0, floatBuf.Length);
			if (read < floatBuf.Length) {
				Reset();
				reader.ReadSamples(floatBuf, read, floatBuf.Length - read);
			}

			Convert(floatBuf, buffer);
		}

		public override void Stop(AudioStopOptions options) {
			base.Stop(options);
			
			reader.Dispose();
			reader = null;
			floatBuf = null;
		}

		public override void Reset() {
			if (reader != null)
				reader.DecodedPosition = 0;
		}

		public static void Convert(float[] floatBuf, byte[] buffer) {
			for (int i = 0; i < floatBuf.Length; i++) {
				short val = (short)(floatBuf[i] * short.MaxValue);
				buffer[i * 2] = (byte)val;
				buffer[i * 2 + 1] = (byte)(val >> 8);
			}
		}
	}
}
