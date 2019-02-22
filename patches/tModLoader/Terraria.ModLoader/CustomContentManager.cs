using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader
{
	// See https://blogs.msdn.microsoft.com/shawnhar/2007/03/09/contentmanager-readasset/ for ideas on implementing a custom ContentManager class.
	class CustomContentManager : ContentManager
	{
		public CustomContentManager(IServiceProvider serviceProvider)
			: base(serviceProvider) { }

		public override T Load<T>(string assetName) {
			return ReadAsset<T>(assetName, null); // We could implement the regular approach and just add a check for Texture.IsDisposed to automatically detect disposed Texture2D, keeping all logic here.
			// Or we can use Main.OurLoad to track things, if we want to unload TexturePack assets. Lots to consider. 
		}
	}
}