using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities;

public partial class TEDisplayDoll
{
	public override void SaveData(TagCompound tag)
	{
		tag["items"] = PlayerIO.SaveInventory(_equip);
		tag["dyes"] = PlayerIO.SaveInventory(_dyes);
		tag["misc"] = PlayerIO.SaveInventory(_misc);
	}

	public override void LoadData(TagCompound tag)
	{
		// TML 1.4.4 saved as "items", 1.4.5 changed field from _items to _equip.
		PlayerIO.LoadInventory(_equip, tag.GetList<TagCompound>("items"));
		PlayerIO.LoadInventory(_dyes, tag.GetList<TagCompound>("dyes"));
		PlayerIO.LoadInventory(_misc, tag.GetList<TagCompound>("misc"));
	}

	public override void NetSend(BinaryWriter writer)
	{
		BitsByte itemsBits = default;
		BitsByte dyesBits = default;
		BitsByte extraBits = default;

		for (int i = 0; i < 8; i++) {
			itemsBits[i] = !_equip[i].IsAir;
			dyesBits[i] = !_dyes[i].IsAir;
		}

		extraBits[0] = !_misc[0].IsAir;
		extraBits[1] = !_equip[8].IsAir;
		extraBits[2] = !_dyes[8].IsAir;

		writer.Write(itemsBits);
		writer.Write(dyesBits);
		writer.Write(_pose);
		writer.Write(extraBits);

		foreach (var item in _equip) {
			if (!item.IsAir) {
				ItemIO.Send(item, writer, true);
			}
		}

		foreach (var item in _dyes) {
			if (!item.IsAir) {
				ItemIO.Send(item, writer, true);
			}
		}

		foreach (var item in _misc) {
			if (!item.IsAir) {
				ItemIO.Send(item, writer, true);
			}
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		BitsByte presentItems = reader.ReadByte();
		BitsByte presentDyes = reader.ReadByte();
		_pose =  reader.ReadByte();
		BitsByte extraBits = reader.ReadByte();

		for (int i = 0; i < 8; i++) {
			_equip[i] = presentItems[i] ? ItemIO.Receive(reader, true) : new Item();
		}
		_equip[8] = extraBits[1] ? ItemIO.Receive(reader, true) : new Item();

		for (int i = 0; i < 8; i++) {
			_dyes[i] = presentDyes[i] ? ItemIO.Receive(reader, true) : new Item();
		}
		_dyes[8] = extraBits[2] ? ItemIO.Receive(reader, true) : new Item();

		_misc[0] = extraBits[0] ? ItemIO.Receive(reader, true) : new Item();
	}
}
