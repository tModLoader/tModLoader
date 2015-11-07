using System;
using System.IO;
using Terraria.ModLoader;

namespace Terraria.ModLoader.Default
{
	public class MysteryItem : ModItem
	{
		private string modName;
		private string itemName;
		private byte[] data;

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

		internal byte[] GetData()
		{
			return data;
		}

		internal void SetData(byte[] data)
		{
			this.data = data;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(modName);
			writer.Write(itemName);
			writer.Write((ushort)data.Length);
			if (data.Length > 0)
			{
				writer.Write(data);
			}
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			SetModName(reader.ReadString());
			SetItemName(reader.ReadString());
			SetData(reader.ReadBytes(reader.ReadUInt16()));
		}
	}
}
