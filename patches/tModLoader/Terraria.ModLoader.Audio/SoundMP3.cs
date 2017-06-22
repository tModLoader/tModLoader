using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using MP3Sharp;

namespace Terraria.ModLoader
{
	public class SoundMP3:IDisposable
	{
		private MP3Stream stream;
		private MemoryStream data;
		private long bytesPerChunk;
		private const int DEFAULT_BYTESPERCHUNK=8192;

		private bool IsLooped;//loop=true allows for only one instance playing of that sound at a time, false buffers entire file

		public SoundMP3(string path,bool loop,int bytesPerChunk=DEFAULT_BYTESPERCHUNK)
		{
			stream=new MP3Stream(path,bytesPerChunk);
			IsLooped=loop;
			this.bytesPerChunk=loop?bytesPerChunk:stream.Length;
		}

		public SoundMP3(byte[] byteArray,bool loop,int bytesPerChunk=DEFAULT_BYTESPERCHUNK)
		{
			data=new MemoryStream(byteArray);
			stream=new MP3Stream(data);
			IsLooped=loop;
			this.bytesPerChunk=loop?bytesPerChunk:stream.Length;
		}

		public void Dispose()
		{
			stream.Close();
			stream=null;
		}

		public DynamicSoundEffectInstance CreateInstance()
		{
			DynamicSoundEffectInstance effect;
			effect=new DynamicSoundEffectInstance(stream.Frequency,(AudioChannels)stream.ChannelCount);
			effect.BufferNeeded+=instance_BufferNeeded;
			return effect;
		}

		public static SoundMP3 FromByteArray(byte[] data)
		{
			if(data==null){throw new ArgumentNullException("stream");}
			return new SoundMP3(data,true);
		}

		//This method is loosely based on code from the XNAMP3 class by ZaneDubya from the MP3Sharp GitHub
		private void instance_BufferNeeded(object sender,EventArgs e)
		{
			if(stream==null)
			{
				((DynamicSoundEffectInstance)sender).Stop();
				return;
			}
			for(int i=3;i>0;i--)
			{
				if(!AddToBuffer((DynamicSoundEffectInstance)sender))
				{
					if(i==3)
					{
						((DynamicSoundEffectInstance)sender).Stop();
					}
					break;
				}
			}
		}

		private bool AddToBuffer(DynamicSoundEffectInstance sound)
		{
			byte[] buffer=new byte[bytesPerChunk];
			int bytesReturned=stream.Read(buffer,0,buffer.Length);
			if(bytesReturned<bytesPerChunk)
			{
				if(IsLooped)
				{
					stream.Position=0;
					stream.Read(buffer,bytesReturned,buffer.Length-bytesReturned);
				}
				else if(bytesReturned==0)
				{
					return false;
				}
			}
			sound.SubmitBuffer(buffer);
			return true;
		}
	}
}