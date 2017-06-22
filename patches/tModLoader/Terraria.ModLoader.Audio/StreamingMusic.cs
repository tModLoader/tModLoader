using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using MP3Sharp;

namespace Terraria.ModLoader.Audio
{
	public class StreamingMusic : IDisposable
	{
		private Stream m_Stream;
		private DynamicSoundEffectInstance m_Instance;
		private long dataStart;

		private const int NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK = 4096;
		private readonly byte[] m_WaveBuffer = new byte[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK];

		public SoundState State => m_Instance.State;
		public bool IsDisposed => m_Instance.IsDisposed;
		public bool IsLooped => m_Instance.IsLooped;
		public float Volume
		{
			set { m_Instance.Volume = value; }
		}

		public StreamingMusic(Stream stream)
		{
			m_Stream = stream;
			Setup();
		}

		public StreamingMusic(string path)
		{
			m_Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			Setup();
		}

		private void Setup()
		{
			BinaryReader reader = new BinaryReader(m_Stream);

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
			m_Stream.Position = dataStart;

			//m_Instance = new DynamicSoundEffectInstance(m_Stream.Frequency, AudioChannels.Stereo);
			m_Instance = new DynamicSoundEffectInstance(sampleRate, (AudioChannels)channels);
		}

		public void Dispose()
		{
			if (m_Instance.State == SoundState.Playing)
			{
				Stop();
			}

			m_Instance.Dispose();
			m_Instance = null;

			m_Stream.Close();
			m_Stream = null;
		}

		public void Play(bool repeat = true)
		{
			if (m_Instance.State == SoundState.Playing)
			{
				Stop();
			}

			m_Instance.IsLooped = repeat;

			SubmitBuffer(1);
			m_Instance.BufferNeeded += instance_BufferNeeded;
			m_Instance.Play();
		}

		public void Pause()
		{
			m_Instance.Pause();
		}

		public void Resume()
		{
			m_Instance.Resume();
		}

		public void Stop(bool immeditate = false)
		{
			if (m_Instance.State == SoundState.Playing)
			{
				m_Instance.Stop(immeditate);
				m_Stream.Position = dataStart;
				m_Instance.BufferNeeded -= instance_BufferNeeded;
			}
		}

		private void instance_BufferNeeded(object sender, EventArgs e)
		{
			SubmitBuffer();
		}

		private void SubmitBuffer(int count = 1)
		{
			while (count > 0)
			{
				ReadFromStream();
				m_Instance.SubmitBuffer(m_WaveBuffer);
				count--;
			}
		}

		private void ReadFromStream()
		{
			int bytesReturned = m_Stream.Read(m_WaveBuffer, 0, m_WaveBuffer.Length);
			if (bytesReturned != NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK)
			{
				if (m_Instance.IsLooped)
				{
					m_Stream.Position = dataStart;
					m_Stream.Read(m_WaveBuffer, bytesReturned, m_WaveBuffer.Length - bytesReturned);
				}
				else
				{
					if (bytesReturned == 0)
					{
						Stop();
					}
				}
			}
		}
	}
}