using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;

namespace Terraria.ModLoader.IO;

internal static partial class TileIO
{
	//*********** Containers (*annequin) Save, Load, and Placeholder Implementations ***********************************//

	//in Terraria.IO.WorldFile.SaveWorldTiles for saving tile frames add
	//  short frameX = tile.frameX; TileIO.VanillaSaveFrames(tile, ref frameX);
	//  and replace references to tile.frameX with frameX
	internal static void VanillaSaveFrames(Tile tile, ref short frameX)
	{
		if (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin) {
			int slot = tile.frameX / 100;
			int position = tile.frameY / 18;
			if (HasModArmor(slot, position)) {
				frameX %= 100;
			}
		}
	}

	internal struct ContainerTables
	{
		internal IDictionary<int, int> headSlots;
		internal IDictionary<int, int> bodySlots;
		internal IDictionary<int, int> legSlots;

		internal static ContainerTables Create()
		{
			ContainerTables tables = new ContainerTables {
				headSlots = new Dictionary<int, int>(),
				bodySlots = new Dictionary<int, int>(),
				legSlots = new Dictionary<int, int>()
			};
			return tables;
		}
	}
	//in Terraria.GameContent.Tile_Entities.TEItemFrame.WriteExtraData
	//  if item is a mod item write 0 as the ID
	internal static TagCompound SaveContainers()
	{
		var ms = new MemoryStream();
		var writer = new BinaryWriter(ms);
		byte[] flags = new byte[1];
		byte numFlags = 0;
		ISet<int> headSlots = new HashSet<int>();
		ISet<int> bodySlots = new HashSet<int>();
		ISet<int> legSlots = new HashSet<int>();
		for (int i = 0; i < Main.maxTilesX; i++) {
			for (int j = 0; j < Main.maxTilesY; j++) {
				Tile tile = Main.tile[i, j];
				if (tile.active() && (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin)) {
					int slot = tile.frameX / 100;
					int position = tile.frameY / 18;
					if (HasModArmor(slot, position)) {
						if (position == 0) {
							headSlots.Add(slot);
						}
						else if (position == 1) {
							bodySlots.Add(slot);
						}
						else if (position == 2) {
							legSlots.Add(slot);
						}
						flags[0] |= 1;
						numFlags = 1;
					}
				}
			}
		}

		int tileEntity = 0;
		List<TagCompound> itemFrames = new List<TagCompound>();
		foreach (KeyValuePair<int, TileEntity> entity in TileEntity.ByID) {
			if (entity.Value is TEItemFrame itemFrame) {
				var globalData = ItemIO.SaveGlobals(itemFrame.item);
				if (globalData != null || ItemLoader.NeedsModSaving(itemFrame.item)) {
					itemFrames.Add(new TagCompound {
						["id"] = tileEntity,
						["item"] = ItemIO.Save(itemFrame.item, globalData)
					});
					//flags[0] |= 2; legacy
					numFlags = 1;
				}
			}
			if(!(entity.Value is ModTileEntity))
				tileEntity++;
		}

		if (numFlags == 0) {
			return null;
		}
		writer.Write(numFlags);
		writer.Write(flags, 0, numFlags);
		if ((flags[0] & 1) == 1) {
			writer.Write((ushort)headSlots.Count);
			foreach (int slot in headSlots) {
				writer.Write((ushort)slot);
				ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Head][slot]);
				writer.Write(item.Mod.Name);
				writer.Write(item.Name);
			}
			writer.Write((ushort)bodySlots.Count);
			foreach (int slot in bodySlots) {
				writer.Write((ushort)slot);
				ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Body][slot]);
				writer.Write(item.Mod.Name);
				writer.Write(item.Name);
			}
			writer.Write((ushort)legSlots.Count);
			foreach (int slot in legSlots) {
				writer.Write((ushort)slot);
				ModItem item = ItemLoader.GetItem(EquipLoader.slotToId[EquipType.Legs][slot]);
				writer.Write(item.Mod.Name);
				writer.Write(item.Name);
			}
			WriteContainerData(writer);
		}
		var tag = new TagCompound();
		tag.Set("data", ms.ToArray());

		if (itemFrames.Count > 0) {
			tag.Set("itemFrames", itemFrames);
		}
		return tag;
	}

	internal static void LoadContainers(TagCompound tag)
	{
		if (tag.ContainsKey("data"))
			ReadContainers(new BinaryReader(tag.GetByteArray("data").ToMemoryStream()));

		foreach (var frameTag in tag.GetList<TagCompound>("itemFrames")) {
			if (TileEntity.ByID.TryGetValue(frameTag.GetInt("id"), out TileEntity tileEntity) && tileEntity is TEItemFrame itemFrame)
				ItemIO.Load(itemFrame.item, frameTag.GetCompound("item"));
			else
				Logging.tML.Warn($"Due to a bug in previous versions of tModLoader, the following ItemFrame data has been lost: {frameTag.ToString()}");
		}
	}

	internal static void ReadContainers(BinaryReader reader)
	{
		byte[] flags = new byte[1];

		reader.Read(flags, 0, reader.ReadByte());

		if ((flags[0] & 1) == 1) {
			var tables = ContainerTables.Create();
			int count = reader.ReadUInt16();

			for (int k = 0; k < count; k++) {
				tables.headSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.headSlot : 0;
			}

			count = reader.ReadUInt16();

			for (int k = 0; k < count; k++) {
				tables.bodySlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.bodySlot : 0;
			}

			count = reader.ReadUInt16();

			for (int k = 0; k < count; k++) {
				tables.legSlots[reader.ReadUInt16()] = ModContent.TryFind(reader.ReadString(), reader.ReadString(), out ModItem item) ? item.Item.legSlot : 0;
			}

			ReadContainerData(reader, tables);
		}

		//legacy load //Let's not care anymore.
		/*if ((flags[0] & 2) == 2) {
			int count = reader.ReadInt32();
			for (int k = 0; k < count; k++) {
				int id = reader.ReadInt32();
				TEItemFrame itemFrame = TileEntity.ByID[id] as TEItemFrame;
				ItemIO.LoadLegacy(itemFrame.item, reader, true);
			}
		}*/
	}

	internal static void WriteContainerData(BinaryWriter writer)
	{
		for (int i = 0; i < Main.maxTilesX; i++) {
			for (int j = 0; j < Main.maxTilesY; j++) {
				Tile tile = Main.tile[i, j];
				if (tile.active() && (tile.type == TileID.Mannequin || tile.type == TileID.Womannequin)) {
					int slot = tile.frameX / 100;
					int frameX = tile.frameX % 100;
					int position = tile.frameY / 18;
					if (HasModArmor(slot, position) && frameX % 36 == 0) {
						writer.Write(i);
						writer.Write(j);
						writer.Write((byte)position);
						writer.Write((ushort)slot);
					}
				}
			}
		}
		writer.Write(-1);
	}

	internal static void ReadContainerData(BinaryReader reader, ContainerTables tables)
	{
		int i = reader.ReadInt32();
		while (i > 0) {
			int j = reader.ReadInt32();
			int position = reader.ReadByte();
			int slot = reader.ReadUInt16();
			Tile left = Main.tile[i, j];
			Tile right = Main.tile[i + 1, j];
			if (left.active() && right.active() && (left.type == TileID.Mannequin || left.type == TileID.Womannequin)
				&& left.type == right.type && (left.frameX == 0 || left.frameX == 36) && right.frameX == left.frameX + 18
				&& left.frameY / 18 == position && left.frameY == right.frameY) {
				if (position == 0) {
					slot = tables.headSlots[slot];
				}
				else if (position == 1) {
					slot = tables.bodySlots[slot];
				}
				else if (position == 2) {
					slot = tables.legSlots[slot];
				}
				left.frameX += (short)(100 * slot);
			}
			i = reader.ReadInt32();
		}
	}

	private static bool HasModArmor(int slot, int position)
	{
		if (position == 0) {
			return slot >= ArmorIDs.Head.Count;
		}
		else if (position == 1) {
			return slot >= ArmorIDs.Body.Count;
		}
		else if (position == 2) {
			return slot >= ArmorIDs.Legs.Count;
		}
		return false;
	}
}
