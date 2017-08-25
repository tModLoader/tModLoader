using System;
using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	public abstract class Music
	{
		public static implicit operator Music(Cue cue) { return new MusicCue() { cue = cue }; }
		public static implicit operator Music(SoundWrapper sound)
		{
			if (sound.IsWAV) { return new MusicWAV(sound.soundWAV); }
			if (sound.IsMP3) { return new MusicMP3(sound.soundMP3); }
			return null;
		}
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

	public class MusicWAV : Music
	{
		internal SoundEffectInstance instance;
		public MusicWAV(SoundWrapper sound)
		{
			instance = sound.CreateInstance();
		}
		//public static implicit operator SoundEffectInstance(MusicSound musicSound){return musicSound.sound;}
		public override bool IsDisposed { get { return instance.IsDisposed; } }
		public override bool IsPaused { get { return instance.State == SoundState.Paused; } }
		public override bool IsPlaying { get { return instance.State != SoundState.Stopped; } }
		public override void Pause() { instance.Pause(); }
		public override void Play() { instance.Play(); }
		public override void Resume() { instance.Resume(); }
		public override void Stop(AudioStopOptions options) { instance.Stop(options == AudioStopOptions.Immediate); }
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
	}
	public class MusicMP3 : Music
	{
		private const int bufferMin = 3;
		private const int bufferCountPerSubmit = 3;
		internal SoundMP3 sound;
		internal DynamicSoundEffectInstance instance;
		public MusicMP3(SoundMP3 sound)
		{
			this.sound = sound;
			instance = sound.CreateInstance();
		}
		//public static implicit operator SoundEffectInstance(MusicSound musicSound){return musicSound.sound;}
		public override bool IsDisposed { get { return instance.IsDisposed; } }
		public override bool IsPaused { get { return instance.State == SoundState.Paused; } }
		public override bool IsPlaying { get { return instance.State != SoundState.Stopped; } }
		public override void Pause() { instance.Pause(); }
		public override void Play() { instance.Play(); }
		public override void Resume() { instance.Resume(); }
		public override void Stop(AudioStopOptions options)
		{
			instance.Stop(options == AudioStopOptions.Immediate);
			sound.ResetStreamPosition();
		}
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
		public override void CheckBuffer()
		{
			if (instance.PendingBufferCount < bufferMin && IsPlaying)
			{
				sound.SubmitBuffer(instance, bufferCountPerSubmit);
			}
		}
	}
}
