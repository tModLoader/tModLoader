using System.IO;

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

		internal void Setup(string modName, string itemName, byte[] data)
		{
			this.modName = modName;
			this.itemName = itemName;
			this.data = data;
			item.toolTip = "Mod: " + modName;
			item.toolTip2 = "Item: " + itemName;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(modName);
			writer.Write(itemName);
			writer.Write((ushort)data.Length);
			writer.Write(data);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			Setup(reader.ReadString(), reader.ReadString(), reader.ReadBytes(reader.ReadUInt16()));

			var type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
			if (type != 0)
			{
				item.netDefaults(type);
				if (data.Length > 0)
					using (BinaryReader customReader = new BinaryReader(new MemoryStream(data)))
						item.modItem.LoadCustomData(customReader);
				
			}
		}
	}
}
