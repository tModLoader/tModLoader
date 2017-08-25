//#define DEBUG_JUMP_TO_END
using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using MP3Sharp;

namespace Terraria.ModLoader.Audio
{
	public class SoundMP3 : IDisposable
	{
		private MP3Stream stream;
		private MemoryStream data;
		private long bytesPerChunk;
		private const int DEFAULT_BYTESPERCHUNK = 4096;//8192;

		public SoundMP3(byte[] byteArray, /*bool music,*/ int bytesPerChunk = DEFAULT_BYTESPERCHUNK)
		{
			if (byteArray == null) { throw new ArgumentNullException("byteArray"); }
			data = new MemoryStream(byteArray);
			stream = new MP3Stream(data, bytesPerChunk);
			//IsMusic = music;
			this.bytesPerChunk = /*IsMusic ?*/ bytesPerChunk /*: stream.Length*/;
		}

		public void Dispose()
		{
			stream.Close();
			stream = null;
		}

		public DynamicSoundEffectInstance CreateInstance()
		{
			DynamicSoundEffectInstance effect;
			effect = new DynamicSoundEffectInstance(stream.Frequency, (AudioChannels)stream.ChannelCount);
			//int index=SetupReferenceIndex(effect);
			/*if (!IsMusic)
			{
				SubmitBuffer(effect, 1);
				effect.BufferNeeded += sound_BufferNeeded;
			}*/
			//else{effect.BufferNeeded+=music_BufferNeeded;}//{delegate(object sender,EventArgs e){SubmitBuffer((DynamicSoundEffectInstance)sender,index,3);};}

#if DEBUG_JUMP_TO_END
			/*position[index]*/stream.Position=7*stream.Length/8;//Jumps to the end of most songs
#endif
			return effect;
		}

		private void sound_BufferNeeded(object sender, EventArgs e)
		{
			((DynamicSoundEffectInstance)sender).Stop();
		}

		private void music_BufferNeeded(object sender, EventArgs e)
		{
			SubmitBuffer((DynamicSoundEffectInstance)sender, 3);
		}

		internal void SubmitBuffer(DynamicSoundEffectInstance sound/*,int index*/, int count)
		{
			if (stream == null)//Probably don't need to check for this
			{
				sound.Stop();
				return;
			}
			//stream.Position=position[index];
			for (int i = 0; i < count; i++)
			{
				if (!SubmitSingle(sound))
				{
					if (i == 0)
					{
						sound.Stop();
					}
					break;
				}
			}
			//position[index]=stream.Position;
		}

		//Loosely based on code from the XNAMP3 class by ZaneDubya from the MP3Sharp GitHub
		private bool SubmitSingle(DynamicSoundEffectInstance sound)
		{
			byte[] buffer = new byte[bytesPerChunk];
			int bytesReturned = stream.Read(buffer, 0, buffer.Length);
			/*if (!IsMusic)
			{
				ResetStreamPosition();
			}
			else */if (bytesReturned < bytesPerChunk)
			{
				stream.Read(buffer, bytesReturned, buffer.Length - bytesReturned);
				ResetStreamPosition();
			}
			sound.SubmitBuffer(buffer);
			return true;
		}

		public void ResetStreamPosition()
		{
			if (stream != null)
			{
				stream.Position = 0;
				stream.IsEOF = false;
			}
		}
	}
}
