using ExampleMod.Content.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons;

public class ExampleCustomDrawnSword : ModItem
{

	/*
	 * Does not take into account gravity direction nor a velocity limit.
	 */

	public override void SetStaticDefaults() {
		DisplayName.SetDefault("Custom Drawn Sword");
		Tooltip.SetDefault("This is a modded sword with a changing color and custom use and hold styles."); // The (English) text shown below your weapon's name.

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		Item.width = 40;
		Item.height = 40;

		Item.useStyle = ItemUseStyleID.Custom; // This is important. Without it, Pre- and PostDraw are not called at the appropriate times.
		Item.holdStyle = ItemHoldStyleID.Custom;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.autoReuse = true;

		Item.DamageType = DamageClass.Melee;
		Item.damage = 50;
		Item.knockBack = 6;
		Item.crit = 6;

		Item.rare = ModContent.RarityType<ExampleModRarity>();
		Item.UseSound = SoundID.Item1;
	}

	public override bool? UseItem(Player player) {
		player.velocity += new Vector2(5 * player.direction, 0); // Lunge

		return true;
	}

	public override void UseStyle(Player player, Rectangle heldItemFrame) {
		player.bodyFrame.Y = player.bodyFrame.Height * 3;
	}

	public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
		Vector2 size = new Vector2(40, 20);
		Vector2 pos = new Vector2(player.position.X, player.position.Y); // Origin at player

		pos += player.direction == 1f ? new Vector2(player.width, 0) : new Vector2(-size.X, 0); // Adjust for direction
		pos += new Vector2(0, 10); // Add some offset

		hitbox = new Rectangle((int)(pos.X), (int)(pos.Y), (int)size.X, (int)size.Y);
	}

	public override bool PreDrawHeldItem(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;

		// Draw when the item is used
		if (player.ItemAnimationActive) {
			DrawUse(ref drawInfo);
			return false;
		}

		DrawHold(ref drawInfo);
		return false;
	}

	private void DrawUse(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;

		Texture2D texture = TextureAssets.Item[Item.type].Value;
		Vector2 pos = player.position - Main.screenPosition +
		              new Vector2(player.width * 0.5f, 0); // Top Center of player

		pos += new Vector2(20 * player.direction, 20); // Some offset

		SpriteEffects effect = player.direction == 1f
			? SpriteEffects.None
			: SpriteEffects.FlipHorizontally;

		Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height);

		Vector2 origin = new Vector2(10, 30); // Origin at handle

		DrawData drawData = new DrawData(
			texture,
			pos,
			frame,
			drawInfo.itemColor,
			0.5f * player.direction,
			frame.OriginFlip(origin, effect),
			1,
			effect,
			0
		);

		drawInfo.DrawDataCache.Add(drawData);
	}

	private void DrawHold(ref PlayerDrawSet drawInfo) {
		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

		Vector2 pos = drawInfo.drawPlayer.position - Main.screenPosition + new Vector2(10, 0);

		DrawData drawData = new DrawData(
			texture,
			pos,
			new Rectangle(0, 0, texture.Width, texture.Height),
			drawInfo.itemColor,
			0,
			new Vector2(texture.Width * 0.5f, 0),
			1,
			drawInfo.drawPlayer.direction == 1f
				? SpriteEffects.FlipVertically
				: (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically),
			0
		);

		// Drawing this way means the data is not tied to the player and will be drawn behind it. Useful in cases like this.
		// Note that PostDraw changes to the player data are also not applied.
		Main.EntitySpriteDraw(drawData);

		// drawInfo.DrawDataCache.Add(drawData); Drawing this way would apply the rainbow color from PostDraw.
	}


	public override void PostDrawHeldItem(ref PlayerDrawSet drawInfo, List<DrawData> heldItemDrawData) {
		// Change all draw data associated with the holding to use the disco color.
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