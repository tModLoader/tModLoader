using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	public interface IContentSourceExt : IContentSource
	{
		bool HasAsset<T>(string assetName) where T : class;
	}
}
