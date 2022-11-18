using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities;

public partial class TEDisplayDoll
{
	public override void SaveData(TagCompound tag)
	{
		tag["items"] = PlayerIO.SaveInventory(_items);
		tag["dyes"] = PlayerIO.SaveInventory(_dyes);
	}

	public override void LoadData(TagCompound tag)
	{
		PlayerIO.LoadInventory(_items, tag.GetList<TagCompound>("items"));
		PlayerIO.LoadInventory(_dyes, tag.GetList<TagCompound>("dyes"));
	}

	public override void NetSend(BinaryWriter writer)
	{
		BitsByte itemsBits = default;
		BitsByte dyesBits = default;

		for (int i = 0; i < 8; i++) {
			itemsBits[i] = !_items[i].IsAir;
			dyesBits[i] = !_dyes[i].IsAir;
		}

		writer.Write(itemsBits);
		writer.Write(dyesBits);

		for (int i = 0; i < 8; i++) {
			var item = _items[i];

			if (!item.IsAir) {
				ItemIO.Send(item, writer, true);
			}
		}

		for (int i = 0; i < 8; i++) {
			var dye = _dyes[i];

			if (!dye.IsAir) {
				ItemIO.Send(dye, writer, true);
			}
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		BitsByte presentItems = reader.ReadByte();
		BitsByte presentDyes = reader.ReadByte();

		for (int i = 0; i < 8; i++) {
			_items[i] = presentItems[i] ? ItemIO.Receive(reader, true) : new Item();
		}

		for (int i = 0; i < 8; i++) {
			_dyes[i] = presentDyes[i] ? ItemIO.Receive(reader, true) : new Item();
		}
	}
}
