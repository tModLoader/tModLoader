using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities;

public partial class TEHatRack
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

	//NOTE: _items length is 2, so we can compress it to one bitsbyte
	public override void NetSend(BinaryWriter writer)
	{
		BitsByte itemsBits = default;

		for (int i = 0; i < _items.Length; i++) {
			itemsBits[i] = !_items[i].IsAir;
			itemsBits[i + _items.Length] = !_dyes[i].IsAir;
		}

		writer.Write(itemsBits);

		for (int i = 0; i < _items.Length; i++) {
			var item = _items[i];

			if (!item.IsAir) {
				ItemIO.Send(item, writer, true);
			}
		}

		for (int i = 0; i < _dyes.Length; i++) {
			var dye = _dyes[i];

			if (!dye.IsAir) {
				ItemIO.Send(dye, writer, true);
			}
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		BitsByte presentItems = reader.ReadByte();

		for (int i = 0; i < _items.Length; i++) {
			_items[i] = presentItems[i] ? ItemIO.Receive(reader, true) : new Item();
		}

		for (int i = 0; i < _dyes.Length; i++) {
			_dyes[i] = presentItems[i + _items.Length] ? ItemIO.Receive(reader, true) : new Item();
		}
	}
}
