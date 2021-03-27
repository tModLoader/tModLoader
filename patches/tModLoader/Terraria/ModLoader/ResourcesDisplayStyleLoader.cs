using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;

namespace Terraria.ModLoader
{
	public class ResourcesDisplayStyleLoader
	{
		// There are no display set IDs, they're stored in their own list
		// There are 3 of them
		public const int VanillaDisplayStyleCount = 4;

		public static int DisplayStyleCount => VanillaDisplayStyleCount + displayStyles.Count;
		internal static readonly List<ModResourcesDisplayStyle> displayStyles = new List<ModResourcesDisplayStyle>();

		internal static int Add(ModResourcesDisplayStyle displayStyle) {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding display styles breaks vanilla client compatibility");

			displayStyles.Add(displayStyle);
			Main.PlayerResourcesSets.Add(displayStyle.SetName, displayStyle);

			return DisplayStyleCount - 1;
		}

		internal static ModResourcesDisplayStyle GetResourceDisplayStyle(int type) {
			return type >= VanillaDisplayStyleCount && type < DisplayStyleCount ? displayStyles[type - VanillaDisplayStyleCount] : null;
		}

		internal static void GotoSavedDisplayStyle() {
			string reqSet = Main.Configuration.Get("RequestedResourcesSet", "Default");

			if (ModContent.TryFind(reqSet, out ModResourcesDisplayStyle moddedStyle) && Main.PlayerResourcesSets.TryGetValue(moddedStyle.SetName, out IPlayerResourcesDisplaySet set))
				Main.ActivePlayerResourcesSet = set;
		}

		internal static void Unload() {
			if (Main.ActivePlayerResourcesSet is ModResourcesDisplayStyle)
				Main.ActivePlayerResourcesSet = Main.PlayerResourcesSets["Default"];

			// Clear list of every modded display set
			foreach (ModResourcesDisplayStyle set in Main.PlayerResourcesSets.Values.ToList().OfType<ModResourcesDisplayStyle>())
				Main.PlayerResourcesSets.Remove(((IPlayerResourcesDisplaySet) set).SetName);
		}
	}
}
