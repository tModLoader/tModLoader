using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;

namespace Terraria.ModLoader
{
	public class ResourceDisplaySetLoader
	{
		// There are no display set IDs, they're stored in their own list
		// There are 3 of them
		public const int VanillaDisplaySetCount = 4;

		public static int DisplaySetCount => VanillaDisplaySetCount + displaySets.Count;
		internal static readonly List<ModResourceDisplaySet> displaySets = new List<ModResourceDisplaySet>();

		internal static int Add(ModResourceDisplaySet displaySet) {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding display sets breaks vanilla client compatibility");

			displaySets.Add(displaySet);
			Main.PlayerResourcesSets.Add(displaySet.SetName, displaySet);

			// Check to see if the requested set has been registered yet
			// If it has, switch back to it
			GotoSavedDisplaySet();
			return DisplaySetCount - 1;
		}

		internal static ModResourceDisplaySet GetResourceDisplaySet(int type) {
			return type >= VanillaDisplaySetCount && type < DisplaySetCount ? displaySets[type - VanillaDisplaySetCount] : null;
		}

		private static void GotoSavedDisplaySet() {
			string reqSet = Main.Configuration.Get("RequestedResourcesSet", "Default");

			if (Main.PlayerResourcesSets.ContainsKey(reqSet))
				Main.ActivePlayerResourcesSet = Main.PlayerResourcesSets[reqSet];
		}

		internal static void Unload() {
			if (Main.ActivePlayerResourcesSet is ModResourceDisplaySet)
				Main.ActivePlayerResourcesSet = Main.PlayerResourcesSets["Default"];

			// Clear list of every modded display set
			foreach (ModResourceDisplaySet set in Main.PlayerResourcesSets.Values.ToList().OfType<ModResourceDisplaySet>())
				Main.PlayerResourcesSets.Remove(((IPlayerResourcesDisplaySet) set).SetName);
		}
	}
}
