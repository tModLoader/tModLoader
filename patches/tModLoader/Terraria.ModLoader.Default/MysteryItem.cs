using System;
using System.IO;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryItem : ModItem
	{
		private string modName;
		private string itemName;
		private byte[] data;
		private bool hasGlobalData;

		public override void SetDefaults()
		{
			item.name = "Unloaded Item";
			item.width = 20;
			item.height = 20;
			item.rare = 1;
		}

		internal void Setup(string modName, string itemName, byte[] data, bool hasGlobal)
		{
			this.modName = modName;
			this.itemName = itemName;
			this.data = data;
			this.hasGlobalData = hasGlobal;
			item.toolTip = "Mod: " + modName;
			item.toolTip2 = "Item: " + itemName;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			if (hasGlobalData)
			{
				writer.Write("");
			}
			writer.Write(modName);
			writer.Write(itemName);
			writer.Write((ushort)data.Length);
			writer.Write(data);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			string modName = reader.ReadString();
			bool hasGlobal = false;
			if (modName.Length == 0)
			{
				hasGlobal = true;
				modName = reader.ReadString();
			}
			Setup(modName, reader.ReadString(), ItemIO.GetCustomData(Int32.MaxValue, reader, hasGlobal), hasGlobal);

			var type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
			if (type != 0)
			{
				item.netDefaults(type);
				ItemIO.ReadCustomData(item, data, hasGlobalData);
			}
		}
	}
}
