using System;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO;

internal class TileEntry : ModBlockEntry
{
	public static Func<TagCompound, TileEntry> DESERIALIZER = tag => new TileEntry(tag);

	public bool frameImportant;

	public TileEntry(ModTile tile) : base(tile)
	{
		frameImportant = Main.tileFrameImportant[tile.Type];
	}

	public TileEntry(TagCompound tag) : base(tag)
	{
		frameImportant = tag.GetBool("framed");
	}

	public override ModBlockType DefaultUnloadedPlaceholder => ModContent.GetInstance<UnloadedSolidTile>();

	public override TagCompound SerializeData()
	{
		var tag = base.SerializeData();
		tag["framed"] = frameImportant;
		return tag;
	}

	protected override ModBlockType GetUnloadedPlaceholder(ushort type)
	{
		if (TileID.Sets.BasicChest[type])
			return ModContent.GetInstance<UnloadedChest>();

		if (TileID.Sets.BasicDresser[type])
			return ModContent.GetInstance<UnloadedDresser>();

		if (TileID.Sets.RoomNeeds.CountsAsChair.Contains(type) ||
			TileID.Sets.RoomNeeds.CountsAsDoor.Contains(type) ||
			TileID.Sets.RoomNeeds.CountsAsTable.Contains(type) ||
			TileID.Sets.RoomNeeds.CountsAsTorch.Contains(type)) {
			return ModContent.GetInstance<UnloadedSupremeFurniture>();
		}

		if (Main.tileSolidTop[type])
			return ModContent.GetInstance<UnloadedSemiSolidTile>();

		if (!Main.tileSolid[type])
			return ModContent.GetInstance<UnloadedNonSolidTile>();

		return DefaultUnloadedPlaceholder;
	}
}
