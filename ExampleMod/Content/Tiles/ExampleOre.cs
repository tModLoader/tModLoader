using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Content.Tiles
{
	public class ExampleOre : ModTile
	{
		public override void SetDefaults() {
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true; // The tile will be affected by spelunker highlighting
			Main.tileOreFinderPriority[Type] = 410; // Metal Detector value, see https://terraria.gamepedia.com/Metal_Detector
			Main.tileShine2[Type] = true; // Modifies the draw color slightly.
			Main.tileShine[Type] = 975; // How often tiny dust appear off this tile. Larger is less frequently
			Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("ExampleOre");
			AddMapEntry(new Color(152, 171, 198), name);

			DustType = 84;
			ItemDrop = ModContent.ItemType<Items.Placeable.ExampleOre>();
			SoundType = SoundID.Tink;
			SoundStyle = 1;
			//mineResist = 4f;
			//minPick = 200;
		}
	}
}