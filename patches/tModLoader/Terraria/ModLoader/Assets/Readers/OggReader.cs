using Microsoft.Xna.Framework.Audio;
using NVorbis;
using ReLogic.Content;
using ReLogic.Content.Readers;
using System.IO;

namespace Terraria.ModLoader.Assets;

public class OggReader : IAssetReader
{
	T IAssetReader.FromStream<T>(Stream stream) where T : class
	{
		if (typeof(T) != typeof(SoundEffect))
			throw AssetLoadException.FromInvalidReader<OggReader, T>();

		using var reader = new VorbisReader(stream, true);

		byte[] buffer = new byte[reader.TotalSamples * 2 * reader.Channels];
		float[] floatBuf = new float[buffer.Length / 2];

		reader.ReadSamples(floatBuf, 0, floatBuf.Length);
		Convert(floatBuf, buffer);

		return new SoundEffect(buffer, reader.SampleRate, (AudioChannels)reader.Channels) as T;
	}

	public static void Convert(float[] floatBuf, byte[] buffer)
	{
		for (int i = 0; i < floatBuf.Length; i++) {
			short val = (short)(floatBuf[i] * short.MaxValue);
			buffer[i * 2] = (byte)val;
			buffer[i * 2 + 1] = (byte)(val >> 8);
		}
	}
}
