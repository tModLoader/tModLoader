using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Walls
{
	public class ExampleWall : ModWall
	{
		public override void SetDefaults() {
			Main.wallHouse[Type] = true;
			DustType = ModContent.DustType<Sparkle>();
			ItemDrop = ModContent.ItemType<Items.Placeable.ExampleWall>();
			AddMapEntry(new Color(150, 150, 150));
		}

		public override void MineDamage(int i, int j, Item item, ToolType toolType, int minePower, ref StatModifier powerMod) {
			// Stop this wall from being mineable in pre-hardmode.
			if (!Main.hardMode) {
				powerMod *= 0;
			}
			// Otherwise, make this wall take 20 hits at 100 tool power.
			else {
				powerMod /= 20;
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.4f;
			g = 0.4f;
			b = 0.4f;
		}
	}
}