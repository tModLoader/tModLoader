using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Core
{
	internal class ModContentSource : IContentSource, IDisposable
	{
		public IContentValidator ContentValidator { get; set; }

		private readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();

		private TmodFile file;

		public ModContentSource(Mod mod) {
			file = mod.File;
		}

		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);

		public bool HasAsset(string assetName) => file.HasFile(assetName);
		public string GetExtension(string assetName) => Path.GetExtension(assetName);
		public Stream OpenStream(string assetName) => file.GetStream(assetName);

		public void Dispose()
		{
			file = null;

			ClearRejections();
		}
	}
}
