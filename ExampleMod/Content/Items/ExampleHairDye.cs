using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleHairDye : ModItem
	{
		public override void SetStaticDefaults() {
			// Avoid loading assets on dedicated servers. They don't use graphics cards.
			if (!Main.dedServ) {
				// The following code creates a hair color-returning delegate (anonymous method), and associates it with this item's type Id.
				GameShaders.Hair.BindShader(
					Item.type,
					new LegacyHairShaderData().UseLegacyMethod((Player player, Color newColor, ref bool lighting) => Main.DiscoColor) // Returning Main.DiscoColor will make our hair an animated rainbow. You can return any Color here.
				);
			}

			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.consumable = true;
		}
	}
}