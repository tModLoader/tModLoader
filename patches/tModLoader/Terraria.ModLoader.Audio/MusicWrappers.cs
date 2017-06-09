using System;
using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	//todo: further documentation
	public interface IMusic
	{
		bool IsDisposed{get;}
		bool IsPaused{get;}
		bool IsPlaying{get;}
		void Pause();
		void Play();
		void Resume();
		void Stop(AudioStopOptions options);
		void SetVariable(string name,float value);
	}

	public class MusicCue:IMusic
	{
		private Cue cue;
		public static implicit operator MusicCue(Cue cue){return new MusicCue{cue=cue};}
		public static implicit operator Cue(MusicCue musicCue){return musicCue.cue;}
		public bool IsDisposed{get{return cue.IsDisposed;}}
		public bool IsPaused{get{return cue.IsPaused;}}
		public bool IsPlaying{get{return cue.IsPlaying;}}
		public void Pause(){cue.Pause();}
		public void Play(){cue.Play();}
		public void Resume(){cue.Resume();}
		public void Stop(AudioStopOptions options){cue.Stop(options);}
		public void SetVariable(string name,float value){cue.SetVariable(name,value);}
	}

	public class MusicSound:IMusic
	{
		private SoundEffectInstance sound;
		public static implicit operator MusicSound(SoundEffectInstance sound){return new MusicSound{sound=sound};}
		public static implicit operator SoundEffectInstance(MusicSound musicSound){return musicSound.sound;}
		public bool IsDisposed{get{return sound.IsDisposed;}}
		public bool IsPaused{get{return sound.State==SoundState.Paused;}}
		public bool IsPlaying{get{return sound.State!=SoundState.Stopped;}}
		public void Pause(){sound.Pause();}
		public void Play(){sound.Play();}
		public void Resume(){sound.Resume();}
		public void Stop(AudioStopOptions options){sound.Stop(options==AudioStopOptions.Immediate);}
		public void SetVariable(string name,float value)
		{
			switch(name)
			{
				case "Volume":sound.Volume=value;return;
				case "Pitch":sound.Pitch=value;return;
				case "Pan":sound.Pan=value;return;
				default:throw new Exception("Invalid field: '"+name+"'");
			}
		}
	}

	/*public class MusicWrapper
	{
		internal Cue cue;
		private SoundEffectInstance modMusic;

		internal MusicWrapper(Cue cue)
		{
			this.cue = cue;
		}

		internal MusicWrapper()
		{
		}

		public bool IsDisposed
		{
			get
			{
				if (modMusic != null)
				{
					return modMusic.IsDisposed;
				}
				else
				{
					return cue?.IsDisposed ?? true;
				}
			}
		}

		public bool IsPaused
		{
			get
			{
				if (modMusic != null)
				{
					return modMusic.State == SoundState.Paused;
				}
				else
				{
					return cue?.IsPaused ?? true;
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				if (modMusic != null)
				{
					return modMusic.State != SoundState.Stopped;
				}
				else if (cue != null)
				{
					return cue?.IsPlaying ?? false;
				}
				else
				{
					return false;
				}
			}
		}

		public SoundEffectInstance ModMusic
		{
			get
			{
				return modMusic;
			}
			set
			{
				if (this.IsPlaying)
				{
					this.Stop();
					modMusic = value;
					if (modMusic != null)
					{
						modMusic.IsLooped = true;
					}
					this.Play();
				}
				else
				{
					modMusic = value;
					if (modMusic != null)
					{
						modMusic.IsLooped = true;
					}
				}
			}
		}

		public void Pause()
		{
			if (modMusic != null)
			{
				modMusic.Pause();
			}
			else
			{
				cue?.Pause();
			}
		}

		public void Play()
		{
			if (modMusic != null)
			{
				modMusic.Play();
			}
			else
			{
				cue = Main.soundBank?.GetCue(cue.Name) ?? null;
				cue?.Play();
			}
		}

		public void Resume()
		{
			if (modMusic != null)
			{
				modMusic.Resume();
			}
			else
			{
				cue?.Resume();
			}
		}

		public void Stop()
		{
			if (modMusic != null)
			{
				modMusic.Stop();
			}
			else
			{
				cue?.Stop(AudioStopOptions.Immediate);
			}
		}

		public void Stop(bool immediate)
		{
			if (modMusic != null)
			{
				modMusic.Stop(immediate);
			}
			else
			{
				if (immediate)
				{
					cue?.Stop(AudioStopOptions.Immediate);
				}
				else
				{
					cue?.Stop(AudioStopOptions.AsAuthored);
				}
			}
		}

		public void Stop(AudioStopOptions options)
		{
			if (modMusic != null)
			{
				modMusic.Stop(options.HasFlag(AudioStopOptions.Immediate));
			}
			else
			{
				cue?.Stop(options);
			}
		}

		public void SetVariable(string name, float value)
		{
			if (modMusic != null)
			{
				if (name.Equals("Volume"))
				{
					modMusic.Volume = value;
				}
			}
			else
			{
				cue?.SetVariable(name, value);
			}
		}
		//~SoundEffectInstance();
		//public virtual bool IsLooped { get; set; }
		//public float Pan { get; set; }
		//public float Pitch { get; set; }
		//public SoundState State { get; }
		//public float Volume { get; set; }
		//public void Apply3D(AudioListener[] listeners, AudioEmitter emitter);
		//public void Apply3D(AudioListener listener, AudioEmitter emitter);
		//public void Dispose();
		//protected virtual void Dispose(bool disposing);
		//~Cue();
		//public bool IsCreated { get; }
		//
		//public bool IsPlaying { get; }
		//public bool IsPrepared { get; }
		//public bool IsPreparing { get; }
		//public bool IsStopped { get; }
		//public bool IsStopping { get; }
		//public string Name { get; }
		//public event EventHandler<EventArgs> Disposing;
		//public void Apply3D(AudioListener listener, AudioEmitter emitter);
		//public void Dispose();
		//public float GetVariable(string name);
	}*/
}
