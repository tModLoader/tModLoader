using System;
using System.Reflection;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Chest
	{
		private Item[] ReSetupShop(out int slots, int type) {
			item = Main.ChestLootDatabase.GetById(type).Build(out slots, true);
			NPCLoader.SetupShop(type, this, ref slots);

			return items;
		}
	}
}
