using ExampleMod.Content.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons;

public class ExampleCustomDrawnSword : ModItem
{
	public override void SetDefaults() {
		Item.width = 46;
		Item.height = 46;

		// These are important. Without them, Pre/PostDrawHeldItem are not called at the correct times
		Item.useStyle = ItemUseStyleID.Custom; 
		Item.holdStyle = ItemHoldStyleID.Custom;

		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.autoReuse = true;
		Item.UseSound = SoundID.Item1;

		Item.DamageType = DamageClass.Melee;
		Item.damage = 50;
		Item.knockBack = 6f;
		Item.crit = 6;

		Item.rare = ModContent.RarityType<ExampleModRarity>();
		Item.value = 100;
	}

	public override bool? UseItem(Player player) {
		// Lunging attack in the direction we're travelling
		player.velocity += new Vector2(6f * player.direction, 0f);
		if (player.velocity.Length() > 6f) {
			player.velocity = player.velocity.SafeNormalize(Vector2.Zero) * 5f;
		}

		return true;
	}

	public override void UseStyle(Player player, Rectangle heldItemFrame) {
		// This makes our players body use the correct sprite so that we hold the sword in our hand
		player.bodyFrame.Y = player.bodyFrame.Height * 3;
	}

	public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
		Vector2 size = new Vector2(46, 24);
		Vector2 position = new Vector2(player.position.X, player.position.Y); // Origin at player

		position.X += player.direction == 1 ? player.width : -size.X; // Adjust for direction
		position += new Vector2(0f, 6f); // Add some offset

		hitbox = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
	}

	public override bool PreDrawHeldItem(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;

		// This is how we distinguish between drawing an item being used or just being held
		if (player.ItemAnimationActive) {
			// This method will draw our item in our players hand
			DrawItemUse(ref drawInfo);
			return false;
		}

		// This method will draw our item behind our player, on their back
		DrawItemHold(ref drawInfo);
		return false;
	}

	private void DrawItemUse(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		Texture2D texture = TextureAssets.Item[Item.type].Value;

		Vector2 drawPosition = player.Top - Main.screenPosition;
		drawPosition += new Vector2(20f * player.direction, 20f + player.gfxOffY); // Adding some offset 
		drawPosition = drawPosition.Floor(); // This casts .X and .Y to integers to avoid subpixel movement

		SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		Vector2 origin = texture.Size() / 2f;

		float rotation = 0.5f * player.direction;
		rotation += player.gravDir == 1 ? 0f : 0.5f * player.direction;

		DrawData drawData = new DrawData(texture, drawPosition, null, drawInfo.itemColor, rotation, origin, 1f, effects);
		drawInfo.DrawDataCache.Add(drawData);
	}

	private void DrawItemHold(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

		Vector2 drawPosition = player.position - Main.screenPosition;
		drawPosition += new Vector2(10f, 20f + player.gfxOffY);
		drawPosition = drawPosition.Floor();

		SpriteEffects effects = player.gravDir == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
		effects |= player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		Vector2 origin = texture.Size() / 2f;

		DrawData drawData = new DrawData(texture, drawPosition, null, drawInfo.itemColor, 0f, origin, 1f, effects);

		// Instead of adding our data to drawInfo.DrawDataCache like normal, we draw it ourself here, so our sprite appears behind the player
		// Note: It also doesn't have the rainbow effect applied in our PostDrawHeldItem hook, as we're not adding the draw data to be modified there
		Main.EntitySpriteDraw(drawData);
	}

	public override void PostDrawHeldItem(ref PlayerDrawSet drawInfo, List<DrawData> heldItemDrawData) {
		// This changes all the draw data associated with our held item to be drawn in a rainbow color
		// Our DrawItemUse method only adds one item to our DrawDataCache, but if it added multiple this would affect all of them
		for (int i = 0; i < heldItemDrawData.Count; i++) {
			heldItemDrawData[i] = heldItemDrawData[i] with { color = Main.DiscoColor };
		}
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<ExampleSword>()
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.Register();
	}
}