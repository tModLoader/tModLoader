using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Readers;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Assets;

public class FxcReader : IAssetReader
{
	private readonly GraphicsDevice _graphicsDevice;

	public FxcReader(GraphicsDevice graphicsDevice)
	{
		_graphicsDevice = graphicsDevice;
	}

	public async ValueTask<T> FromStream<T>(Stream stream, MainThreadCreationContext mainThreadCtx) where T : class
	{
		if (typeof(T) != typeof(Effect))
			throw AssetLoadException.FromInvalidReader<FxcReader, T>();

		var ms = new MemoryStream();
		stream.CopyTo(ms);

		await mainThreadCtx;
		Debug.Assert(mainThreadCtx.IsCompleted);

		return new Effect(_graphicsDevice, ms.ToArray()) as T;
	}
}
