using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class CraftGroup
	{
		private static readonly IDictionary<string, CraftGroup> vanillaGroups = new Dictionary<string, CraftGroup>();
		public readonly string Name;
		public readonly string DisplayName;
		public readonly IList<int> Items = new List<int>();

		internal CraftGroup(string name, string displayName, params int[] items)
		{
			Name = name;
			DisplayName = displayName;
			foreach (int id in items)
			{
				Items.Add(id);
			}
		}
		//in Terraria.Recipe rewrite useCraftGroup recipes to check if craft groups contain id
		internal static void ResetVanillaGroups()
		{
			vanillaGroups["Wood"] = new CraftGroup("Wood", Lang.misc[37] + " " + Main.itemName[9],
				9, 619, 620, 621, 911, 1729, 2503, 2504);
			vanillaGroups["IronBar"] = new CraftGroup("IronBar", Lang.misc[37] + " " + Main.itemName[22],
				22, 704);
			vanillaGroups["PressurePlate"] = new CraftGroup("PressurePlate", Lang.misc[37] + " " + Lang.misc[38],
				529, 541, 542, 543, 852, 853, 1151);
			vanillaGroups["Sand"] = new CraftGroup("Sand", Lang.misc[37] + " " + Main.itemName[169],
				169, 408, 1246, 370, 3272);
			vanillaGroups["Fragment"] = new CraftGroup("Fragment", Lang.misc[37] + " " + Lang.misc[51],
				3458, 3456, 3457, 3459);
		}

		public static CraftGroup GetVanillaGroup(string name)
		{
			if (vanillaGroups.ContainsKey(name))
			{
				return vanillaGroups[name];
			}
			else
			{
				return null;
			}
		}
	}
}
