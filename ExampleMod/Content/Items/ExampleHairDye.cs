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
			//Avoid loading assets on dedicated servers. They don't use graphics cards.
			if (!Main.dedServ) {
				//The following code creates a hair color-returning delegate (anonymous method), and associates it with this item's type Id.
				GameShaders.Hair.BindShader(
					item.type,
					new LegacyHairShaderData().UseLegacyMethod((Player player, Color newColor, ref bool lighting) => Main.DiscoColor) //Returning Main.DiscoColor will make our hair an animated rainbow. You can return any Color here.
				);
			}
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 26;
			item.maxStack = 99;
			item.value = Item.buyPrice(gold: 5);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item3;
			item.useStyle = ItemUseStyleID.EatFood;
			item.useTurn = true;
			item.useAnimation = 17;
			item.useTime = 17;
			item.consumable = true;
		}
	}
}