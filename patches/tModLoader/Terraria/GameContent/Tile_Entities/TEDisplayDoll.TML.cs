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
		var bitWriter = new BitWriter();

		foreach (var item in _equip) {
			bitWriter.WriteBit(!item.IsAir);
		}

		foreach (var item in _dyes) {
			bitWriter.WriteBit(!item.IsAir);
		}

		foreach (var item in _misc) {
			bitWriter.WriteBit(!item.IsAir);
		}

		bitWriter.Flush(writer);

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

		writer.Write(_pose);
	}

	public override void NetReceive(BinaryReader reader)
	{
		var bitReader = new BitReader(reader);

		for (int i = 0; i < _equip.Length; ++i) {
			_equip[i] = bitReader.ReadBit() ? ItemIO.Receive(reader, true) : new Item();
		}

		for (int i = 0; i < _dyes.Length; ++i) {
			_dyes[i] = bitReader.ReadBit() ? ItemIO.Receive(reader, true) : new Item();
		}

		for (int i = 0; i < _misc.Length; ++i) {
			_misc[i] = bitReader.ReadBit() ? ItemIO.Receive(reader, true) : new Item();
		}

		_pose = reader.ReadByte();
	}
}
