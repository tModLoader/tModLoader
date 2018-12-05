using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	internal class ExampleGlobalRecipe : GlobalRecipe
	{
		private readonly int slimeKillIndex = Item.NPCtoBanner(NPCID.BlueSlime);

		public override bool RecipeAvailable(Recipe recipe) {
			if (recipe.createItem.createTile == TileID.Torches) {
				return NPC.killCount[slimeKillIndex] >= 10;
			}
			return true;
		}

		private int[] slimeChoices = { NPCID.BlueSlime, NPCID.RainbowSlime, NPCID.SandSlime, NPCID.SlimeSpiked, NPCID.SpikedIceSlime, NPCID.SpikedJungleSlime, NPCID.UmbrellaSlime };

		public override void OnCraft(Item item, Recipe recipe) {
			bool hasGel = false;
			foreach (var requiredItem in recipe.requiredItem) {
				if (requiredItem.stack > 0 && requiredItem.type == ItemID.Gel) {
					hasGel = true;
					break;
				}
			}
			if (hasGel && Main.rand.NextBool(10)) {
				Main.NewText("Revenge for our fallen brothers!!!", Color.Green.R, Color.Green.G, Color.Green.B);
				Player player = Main.LocalPlayer;
				for (int i = 0; i < 5; i++) {
					NPC.NewNPC((int)(player.Center.X - 10 + i * 5), (int)player.Center.Y, slimeChoices[Main.rand.Next(slimeChoices.Length)]);
				}
			}
		}
	}
}
