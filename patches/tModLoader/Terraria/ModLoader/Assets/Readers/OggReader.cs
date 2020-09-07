using Microsoft.Xna.Framework.Audio;
using NVorbis;
using ReLogic.Content;
using ReLogic.Content.Readers;
using System;
using System.IO;
using Terraria.ModLoader.Audio;

namespace Terraria.ModLoader.Assets
{
	public class OggReader : IAssetReader, IDisposable
	{
		public T FromStream<T>(Stream stream) where T : class {
			if (typeof(T) != typeof(SoundEffect))
				throw AssetLoadException.FromInvalidReader<OggReader, T>();

			using var reader = new VorbisReader(stream, true);

			byte[] buffer = new byte[reader.TotalSamples * 2 * reader.Channels];
			float[] floatBuf = new float[buffer.Length / 2];

			reader.ReadSamples(floatBuf, 0, floatBuf.Length);
			MusicStreamingOGG.Convert(floatBuf, buffer);

			return new SoundEffect(buffer, reader.SampleRate, (AudioChannels)reader.Channels) as T;
		}

		public void Dispose() {

		}

		public Type[] GetAssociatedTypes() => new[] { typeof(SoundEffect) };
	}
}
