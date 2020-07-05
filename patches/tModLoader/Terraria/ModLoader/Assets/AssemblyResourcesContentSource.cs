using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	public sealed class AssemblyResourcesContentSource : IContentSource, IDisposable
	{
		private static readonly StringComparer StringComparer = StringComparer.CurrentCultureIgnoreCase;

		public IContentValidator ContentValidator { get; set; }

		private readonly string RootPath;
		private readonly HashSet<string> ResourceNames;
		private readonly RejectedAssetCollection Rejections;
		private readonly Dictionary<string, string> PathRedirects;

		private Assembly assembly;

		public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null)
		{
			RootPath = rootPath;
			Rejections = new RejectedAssetCollection();

			var resourceNames = assembly.GetManifestResourceNames().ToList();

			if (RootPath != null) {
				PathRedirects = new Dictionary<string, string>();

				for (int i = 0;i<resourceNames.Count;i++) {
					string path = resourceNames[i];

					if (path.StartsWith(rootPath)) {
						string shortPath = path.Substring(rootPath.Length);

						resourceNames[i] = shortPath;
						PathRedirects[shortPath] = path;
					} else {
						resourceNames.RemoveAt(i--);
					}
				}
			}

			ResourceNames = new HashSet<string>(resourceNames);

			this.assembly = assembly;
		}

		//Assets
		public bool HasAsset(string assetName) => ResourceNames.Any(s => StringComparer.Equals(s, assetName));
		public string GetExtension(string assetName) => Path.GetExtension(assetName);
		public Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(PathRedirects != null ? PathRedirects[assetName] : assetName);
		public IEnumerable<string> EnumerateFiles() => ResourceNames;
		//Etc
		public void Dispose()
		{
			assembly = null;

			ResourceNames.Clear();
			PathRedirects.Clear();

			ClearRejections();
		}

		//Rejections
		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);
	}
}
