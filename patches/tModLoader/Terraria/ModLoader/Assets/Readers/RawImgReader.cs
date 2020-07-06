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
		//private readonly ThreadLocal<Color[]> _colorProcessingCache;
		//private bool _disposedValue;

		public RawImgReader(GraphicsDevice graphicsDevice) {
			_graphicsDevice = graphicsDevice;
			//_colorProcessingCache = new ThreadLocal<Color[]>();
		}

		public T FromStream<T>(Stream stream) where T : class {
			if (typeof(T) != typeof(Texture2D))
				throw AssetLoadException.FromInvalidReader<RawImgReader, T>();

			Texture2D texture2D = ImageIO.RawToTexture2D(_graphicsDevice, stream);

			/*int num = texture2D.Width * texture2D.Height;

			if (!_colorProcessingCache.IsValueCreated || _colorProcessingCache.Value.Length < num)
				_colorProcessingCache.Value = new Color[num];

			Color[] value = _colorProcessingCache.Value;
			
			texture2D.GetData(value, 0, num);

			for (int i = 0;i != num;i++) {
				value[i] = Color.FromNonPremultiplied(value[i].ToVector4());
			}

			texture2D.SetData(value, 0, num);*/

			return texture2D as T;
		}

		public void Dispose() {

		}

		public Type[] GetAssociatedTypes() => new[] { typeof(Texture2D) };
	}
}
