using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	public class AssemblyResourcesContentSource : IContentSource, IDisposable
	{
		private static readonly StringComparer StringComparer = StringComparer.CurrentCultureIgnoreCase;

		public IContentValidator ContentValidator { get; set; }

		private readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();
		private readonly string[] КesourceNames;

		private Assembly assembly;

		public AssemblyResourcesContentSource(Assembly assembly)
		{
			КesourceNames = assembly.GetManifestResourceNames();

			this.assembly = assembly;
		}

		//Rejections
		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);
		//Assets
		public bool HasAsset(string assetName) => КesourceNames.Any(s => StringComparer.Equals(s, assetName));
		public string GetExtension(string assetName) => Path.GetExtension(assetName);
		public Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(assetName);
		//Etc
		public void Dispose()
		{
			assembly = null;

			ClearRejections();
		}
	}
}
