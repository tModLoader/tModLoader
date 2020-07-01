using System.Collections.Generic;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	public interface IModContentSource : IContentSource
	{
		bool HasAsset<T>(string assetName) where T : class;
		IEnumerable<string> EnumeratePaths<T>() where T : class;
	}
}
