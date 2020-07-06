using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Graphics;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.Assets
{
	public class TModContentSource : IContentSource, IDisposable
	{
		private readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();

		private TmodFile file;

		public IContentValidator ContentValidator { get; set; }

		public TModContentSource(TmodFile file) {
			this.file = file ?? throw new ArgumentNullException(nameof(file));
		}

		//Rejections
		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);
		//Assets
		public bool HasAsset(string assetName)=> file.HasFile(assetName); //This one currently doesn't do any extension guessing
		public string GetExtension(string assetName) => Path.GetExtension(assetName);
		public IEnumerable<string> EnumerateFiles() => file.Select(fileEntry => fileEntry.Name);
		public Stream OpenStream(string assetName) => new MemoryStream(file.GetBytes(assetName)); //This has to return a seekable stream, so we can't just return the deflate one.
		//Etc
		public void Dispose() {
			file = null;

			ClearRejections();
		}
	}
}
