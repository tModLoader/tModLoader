using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using ReLogic.Content.Readers;
using System;
using System.IO;
using XPT.Core.Audio.MP3Sharp;

namespace Terraria.ModLoader.Assets;

public class MP3Reader : IAssetReader
{
	T IAssetReader.FromStream<T>(Stream stream) where T : class
	{
		if (typeof(T) != typeof(SoundEffect))
			throw AssetLoadException.FromInvalidReader<MP3Reader, T>();

		using var mp3Stream = new MP3Stream(stream);
		using var ms = new MemoryStream();

		mp3Stream.CopyTo(ms);

		//TODO: The MP3Sharp library changed much, and no longer has a ChannelCount property. Investigate.
		return new SoundEffect(ms.ToArray(), mp3Stream.Frequency, AudioChannels.Stereo) as T;
	}
}
