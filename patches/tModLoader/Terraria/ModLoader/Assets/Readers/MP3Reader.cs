using Microsoft.Xna.Framework.Audio;
using MP3Sharp;
using ReLogic.Content;
using ReLogic.Content.Readers;
using System;
using System.IO;

namespace Terraria.ModLoader.Assets
{
	public class MP3Reader : IAssetReader, IDisposable
	{
		public T FromStream<T>(Stream stream) where T : class {
			if (typeof(T) != typeof(SoundEffect))
				throw AssetLoadException.FromInvalidReader<MP3Reader, T>();

			using var mp3Stream = new MP3Stream(stream);
			using var ms = new MemoryStream();

			mp3Stream.CopyTo(ms);

			return new SoundEffect(ms.ToArray(), mp3Stream.Frequency, (AudioChannels)mp3Stream.ChannelCount) as T;
		}

		public void Dispose() {

		}

		public Type[] GetAssociatedTypes() => new[] { typeof(SoundEffect) };
	}
}
