using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class CraftGroup
	{
		public static CraftGroup Wood
		{
			get;
			private set;
		}

		public static CraftGroup IronBar
		{
			get;
			private set;
		}

		public static CraftGroup PressurePlate
		{
			get;
			private set;
		}

		public static CraftGroup Sand
		{
			get;
			private set;
		}

		public static CraftGroup Fragment
		{
			get;
			private set;
		}

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
			Wood = new CraftGroup("Wood", Lang.misc[37] + " " + Main.itemName[9],
				9, 619, 620, 621, 911, 1729, 2503, 2504);
			IronBar = new CraftGroup("IronBar", Lang.misc[37] + " " + Main.itemName[22],
				22, 704);
			PressurePlate = new CraftGroup("PressurePlate", Lang.misc[37] + " " + Lang.misc[38],
				529, 541, 542, 543, 852, 853, 1151);
			Sand = new CraftGroup("Sand", Lang.misc[37] + " " + Main.itemName[169],
				169, 408, 1246, 370, 3272);
			Fragment = new CraftGroup("Fragment", Lang.misc[37] + " " + Lang.misc[51],
				3458, 3456, 3457, 3459);
		}
	}
}
