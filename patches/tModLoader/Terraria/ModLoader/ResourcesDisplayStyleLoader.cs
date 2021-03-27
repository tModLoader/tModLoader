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
		public const int VanillaDisplaySetCount = 4;

		public static int DisplaySetCount => VanillaDisplaySetCount + displayStyles.Count;
		internal static readonly List<ModResourcesDisplayStyle> displayStyles = new List<ModResourcesDisplayStyle>();

		internal static int Add(ModResourcesDisplayStyle displayStyle) {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding display sets breaks vanilla client compatibility");

			displayStyles.Add(displayStyle);
			Main.PlayerResourcesSets.Add(displayStyle.SetName, displayStyle);

			// Check to see if the requested set has been registered yet
			// If it has, switch back to it
			GotoSavedDisplaySet();
			return DisplaySetCount - 1;
		}

		internal static ModResourcesDisplayStyle GetResourceDisplayStyle(int type) {
			return type >= VanillaDisplaySetCount && type < DisplaySetCount ? displayStyles[type - VanillaDisplaySetCount] : null;
		}

		private static void GotoSavedDisplaySet() {
			string reqSet = Main.Configuration.Get("RequestedResourcesSet", "Default");

			if (Main.PlayerResourcesSets.ContainsKey(reqSet))
				Main.ActivePlayerResourcesSet = Main.PlayerResourcesSets[reqSet];
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
