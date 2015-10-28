using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	public class MusicWrapper
	{
		public Cue cue;
		private SoundEffectInstance modMusic;

		public MusicWrapper(Cue cue)
		{
			this.cue = cue;
		}

		public MusicWrapper()
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
					return cue.IsDisposed;
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
					return cue.IsPaused;
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
					return cue.IsPlaying;
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
				cue.Pause();
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
				cue = Main.soundBank.GetCue(cue.Name);
				cue.Play();
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
				cue.Resume();
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
				cue.Stop(AudioStopOptions.Immediate);
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
					cue.Stop(AudioStopOptions.Immediate);
				}
				else
				{
					cue.Stop(AudioStopOptions.AsAuthored);
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
				cue.Stop(options);
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
				cue.SetVariable(name, value);
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
	}
}
