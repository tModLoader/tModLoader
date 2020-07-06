using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class Infinity : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Never run out of anything!");
		}

		public override void SetDefaults() {
			item.width = 34;
			item.height = 18;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.Lime;
			item.expert = true;
			item.expertOnly = true; // Makes it so the item's accessory effects only work in expert mode
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ExamplePlayer>().infinity = true;
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White; // So the item's sprite isn't affected by light
		}

		// This gives the item an outline that constantly changes color
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = Main.itemTexture[item.type];
			Vector2 position = item.position - Main.screenPosition + new Vector2(item.width / 2, item.height - texture.Height * 0.5f + 2f);
			// We redraw the item's sprite 4 times, each time shifted 2 pixels on each direction, using Main.DiscoColor to give it the color changing effect
			for (int i = 0; i < 4; i++) {
				Vector2 offsetPositon = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i) * 2;
				spriteBatch.Draw(texture, position + offsetPositon, null, Main.DiscoColor, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			}
			// Return true so the original sprite is drawn right after
			return true;
		}

		// Same as above but for drawing inside the player's inventory
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = Main.itemTexture[item.type];
			for (int i = 0; i < 4; i++) {
				Vector2 offsetPositon = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i) * 2;
				spriteBatch.Draw(texture, position + offsetPositon, null, Main.DiscoColor, 0, origin, scale, SpriteEffects.None, 0f);
			}
			return true;
		}
	}

	public class InfinityGlobalItem : GlobalItem
	{
		public override bool ConsumeItem(Item item, Player player) {
			return !player.GetModPlayer<ExamplePlayer>().infinity;
		}

		public override bool ConsumeAmmo(Item item, Player player) {
			return !player.GetModPlayer<ExamplePlayer>().infinity;
		}

		// Replenishes the mana of the player has soon as they need some, by exactly the amount they need, and stops the mana flower from triggering.
		public override void OnMissingMana(Item item, Player player, int neededMana) {
			if (player.GetModPlayer<ExamplePlayer>().infinity) {
				player.statMana += neededMana;
			}
		}
	}
}
