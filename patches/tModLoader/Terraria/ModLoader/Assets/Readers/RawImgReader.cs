using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Readers;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Assets
{
	public class RawImgReader : IAssetReader, IDisposable
	{
		private readonly GraphicsDevice _graphicsDevice;

		public RawImgReader(GraphicsDevice graphicsDevice) {
			_graphicsDevice = graphicsDevice;
		}

		public T FromStream<T>(Stream stream) where T : class {
			if (typeof(T) != typeof(Texture2D))
				throw AssetLoadException.FromInvalidReader<RawImgReader, T>();

			return ImageIO.RawToTexture2D(_graphicsDevice, stream) as T;
		}

		public void Dispose() {

		}

		public Type[] GetAssociatedTypes() => new[] { typeof(Texture2D) };
	}
}
