using System;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	public class TileEntry : ModEntry
	{
		public static Func<TagCompound, TileEntry> DESERIALIZER = tag => new TileEntry(tag);

		public bool frameImportant;

		public TileEntry(ModTile tile) : base(tile) {
			frameImportant = Main.tileFrameImportant[tile.Type];
		}

		public TileEntry(TagCompound tag) : base(tag) {
			frameImportant = tag.GetBool("framed");
		}

		public override string DefaultUnloadedType => ModContent.GetInstance<UnloadedSolidTile>().FullName;

		public override TagCompound SerializeData() {
			var tag = base.SerializeData();
			tag["framed"] = frameImportant;
			return tag;
		}

		protected override string GetUnloadedType(ushort type) {
			if (TileID.Sets.BasicChest[type])
				return ModContent.GetInstance<UnloadedChest>().FullName;

			if (TileID.Sets.BasicDresser[type])
				return ModContent.GetInstance<UnloadedDresser>().FullName;

			if (Main.tileSolidTop[type])
				return ModContent.GetInstance<UnloadedSemiSolidTile>().FullName;

			if (!Main.tileSolid[type])
				return ModContent.GetInstance<UnloadedNonSolidTile>().FullName;

			return DefaultUnloadedType;
		}
	}
}
