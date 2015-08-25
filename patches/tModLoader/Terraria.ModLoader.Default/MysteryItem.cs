using System;
using System.IO;
using Terraria.ModLoader;

namespace Terraria.ModLoader.Default
{
	public class MysteryItem : ModItem
	{
		private string modName;
		private string itemName;

		public override void SetDefaults()
		{
			item.name = "Unloaded Item";
			item.width = 20;
			item.height = 20;
			item.rare = 1;
		}

		internal string GetModName()
		{
			return modName;
		}

		internal void SetModName(string name)
		{
			modName = name;
			item.toolTip = "Mod: " + name;
		}

		internal string GetItemName()
		{
			return itemName;
		}

		internal void SetItemName(string name)
		{
			itemName = name;
			item.toolTip2 = "Item: " + name;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(modName);
			writer.Write(itemName);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			SetModName(reader.ReadString());
			SetItemName(reader.ReadString());
		}
	}
}
