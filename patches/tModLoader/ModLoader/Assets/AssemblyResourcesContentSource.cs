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

		protected readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();
		protected readonly string[] ResourceNames;

		private Assembly assembly;

		public AssemblyResourcesContentSource(Assembly assembly)
		{
			ResourceNames = assembly.GetManifestResourceNames();

			this.assembly = assembly;
		}

		//Assets
		public virtual bool HasAsset(string assetName) => ResourceNames.Any(s => StringComparer.Equals(s, assetName));
		public virtual string GetExtension(string assetName) => Path.GetExtension(assetName);
		public virtual Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(assetName);
		//Etc
		public virtual void Dispose()
		{
			assembly = null;

			ClearRejections();
		}

		//Rejections
		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);
	}
}
