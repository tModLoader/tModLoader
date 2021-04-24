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
		private readonly List<string> ResourceNames;
		private readonly RejectedAssetCollection Rejections;
		private readonly Dictionary<string, string> PathRedirects;

		private Assembly assembly;

		public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null)
		{
			RootPath = rootPath ?? "";
			Rejections = new RejectedAssetCollection();

			IEnumerable<string> resourceNames = assembly.GetManifestResourceNames();

			if (RootPath != null) {
				resourceNames = resourceNames
					.Where(p => p.StartsWith(RootPath))
					.Select(p => p.Substring(RootPath.Length));
			}

			ResourceNames = resourceNames.ToList();

			this.assembly = assembly;
		}

		//Assets
		public bool HasAsset(string assetName) => ResourceNames.Any(s => StringComparer.Equals(s, assetName));
		public string GetExtension(string assetName) => Path.GetExtension(assetName);
		public Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(RootPath + assetName);
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

		public List<string> GetAllAssetsStartingWith(string assetNameStart) {
			var list = new List<string>();

			foreach (string path in EnumerateFiles()) {
				if (path.StartsWith(assetNameStart, StringComparison.CurrentCultureIgnoreCase))
					list.Add(path);
			}

			return list;
		}
	}
}
