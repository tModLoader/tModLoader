using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Content.Items
{
	/*
	This item showcases proper usage of PreDrawInInventory, PostDrawInInventory, PreDrawInWorld, and PostDrawInWorld. In particular, these examples are intended to show the correct math and arguments needed to draw at the intended positions. World drawing and inventory have specific nuances that modders need to be aware of.

	Right click on the item in the inventory to toggle between each example:
		DrawModeGlowmask: A separate texture, CustomItemDrawingShowcase_Front, is drawn using PostDrawInInventory and PostDrawInWorld
		DrawModePulse: The item texture is drawn manually in PreDrawInInventory and PreDrawInWorld to give the item a pulsing effect similar to Soul items.
		DrawModeBehindTexture: A separate animated texture, CustomItemDrawingShowcase_Back, is drawn using PreDrawInInventory and PreDrawInWorld
		DrawModeHighlightEffect: The item texture is drawn manually several times in PreDrawInWorld to give the item a afterimage aura effect, similar to Boss Bags.
		DrawModeRockingRotation: The item texture is drawn manually.

	Note that these techniques can't be used as is for useable items, such as swords, that would require more advanced logic and hooks.
	When using these techniques, it's a good idea to test your custom inventory drawing code in other UI such as an ItemDefinition field of a ModConfig to make sure the drawing logic is correct at different draw scales.
	*/
	public class CustomItemDrawingShowcase : ModItem
	{
		private static Asset<Texture2D> backTexture;
		private static Asset<Texture2D> frontTexture;

		private int drawMode = 0;
		private const int DrawModeGlowmask = 0;
		private const int DrawModePulse = 1;
		private const int DrawModeBehindTexture = 2;
		private const int DrawModeHighlightAfterImageEffect = 3;
		private const int DrawModeRockingRotation = 4;
		private const int Count = 5;

		public static LocalizedText RightClickText { get; private set; }
		public static LocalizedText[] DrawModeText { get; private set; }

		public override void Load() {
			backTexture = ModContent.Request<Texture2D>(Texture + "_Back");
			frontTexture = ModContent.Request<Texture2D>(Texture + "_Front");
		}

		public override void SetStaticDefaults() {
			RightClickText = this.GetLocalization("RightClick");
			DrawModeText = Enumerable.Range(0, 5).Select(i => this.GetLocalization($"DrawMode_{i}")).ToArray();
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			// Note that Item.width and height are the in-world hitbox dimensions and don't have to match the texture dimensions. Be mindful of this distinction when writing custom item drawing code.
		}

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => false;

		public override void RightClick(Player player) {
			drawMode = (drawMode + 1) % Count;
			Main.NewText(RightClickText.Format(drawMode, GetMessageForDrawMode()));
		}

		private string GetMessageForDrawMode() {
			if (DrawModeText.IndexInRange(drawMode)) {
				return DrawModeText[drawMode].Value;
			}

			return "Unknown mode";
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "DrawModeDetails", $"drawMode #{drawMode}: {GetMessageForDrawMode()}"));
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (drawMode == DrawModePulse) {
				scale *= Main.essScale;
				drawColor *= Main.essScale;
				spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
				return false; // Since we drew the texture, return false so the item isn't drawn twice.
			}
			else if (drawMode == DrawModeBehindTexture) {
				// CustomItemDrawingShowcase_Back.png has different dimensions than CustomItemDrawingShowcase.png, so we need to calculate values for the origin and sourceRectangle parameters to draw correctly

				int backFrameNumber = (int)(Main.GameUpdateCount % 60 / 30);
				var backSourceRectangle = backTexture.Frame(verticalFrames: 2, frameY: backFrameNumber);
				var backOrigin = backSourceRectangle.Size() / 2;

				spriteBatch.Draw(backTexture.Value, position, backSourceRectangle, drawColor, 0, backOrigin, scale, SpriteEffects.None, 0);
			}
			else if (drawMode == DrawModeRockingRotation) {
				float rotation = MathF.Cos(Main.GameUpdateCount * 0.03f) * MathHelper.ToRadians(30); // Rotate left and right at most 30 degrees. 0.03 slows down the rotation speed.

				spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, rotation, origin, scale, SpriteEffects.None, 0);
				return false; // Since we drew the texture, return false so the item isn't drawn twice.
			}

			return true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (drawMode == DrawModeGlowmask) {
				// For a glowmask that is the same dimensions as the item sprite, we can use all the provided parameters as is to draw the glowmask texture.
				spriteBatch.Draw(frontTexture.Value, position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
			}
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
			Vector2 drawOrigin = itemFrame.Size() / 2f;
			// Items in the world are drawn centered horizontally sitting at the bottom of the item hitbox, not in the center.
			Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

			if (drawMode == DrawModePulse) {
				scale = scale * Main.essScale;
				lightColor = lightColor * Main.essScale;
				spriteBatch.Draw(itemTexture, drawPosition, itemFrame, lightColor, rotation, drawOrigin, scale, SpriteEffects.None, 0);
				return false; // Since we drew the texture, return false so the item isn't drawn twice.
			}
			else if (drawMode == DrawModeBehindTexture) {
				// CustomItemDrawingShowcase_Back.png has different dimensions than CustomItemDrawingShowcase.png, so we need to calculate values for the origin and sourceRectangle parameters to draw correctly

				int backFrameNumber = (int)(Main.GameUpdateCount % 60 / 30);
				var backSourceRectangle = backTexture.Frame(verticalFrames: 2, frameY: backFrameNumber);
				var backOrigin = backSourceRectangle.Size() / 2f;

				spriteBatch.Draw(backTexture.Value, drawPosition, backSourceRectangle, lightColor, rotation, backOrigin, scale, SpriteEffects.None, 0);
			}
			else if (drawMode == DrawModeHighlightAfterImageEffect) {
				// This code is a copy of the ItemID.Sets.BossBag effect code from Main.DrawItem. We wouldn't want to use ItemID.Sets.BossBag to get this effect for this item since ItemID.Sets.BossBag has other effects as well that we don't want.
				float counter = Item.timeSinceItemSpawned / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
				float offsetScale = Main.GlobalTimeWrappedHourly;
				offsetScale %= 4f;
				offsetScale /= 2f;
				if (offsetScale >= 1f) {
					offsetScale = 2f - offsetScale;
				}

				offsetScale = offsetScale * 0.5f + 0.5f;
				// 4 far afterimages
				for (float i = 0f; i < 1f; i += 0.25f) {
					spriteBatch.Draw(itemTexture, drawPosition + new Vector2(0f, 8f).RotatedBy((i + counter) * ((float)Math.PI * 2f)) * offsetScale, itemFrame, new Color(90, 70, 255, 50), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
				}
				// 3 close afterimages
				for (float i = 0f; i < 1f; i += 0.34f) {
					spriteBatch.Draw(itemTexture, drawPosition + new Vector2(0f, 4f).RotatedBy((i + counter) * ((float)Math.PI * 2f)) * offsetScale, itemFrame, new Color(140, 120, 255, 77), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
				}
			}
			else if (drawMode == DrawModeRockingRotation) {
				rotation += MathF.Cos(Main.GameUpdateCount * 0.03f) * MathHelper.ToRadians(30);
			}

			return true;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
			Vector2 origin = itemFrame.Size() / 2f;
			Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, origin.Y);

			if (drawMode == DrawModeGlowmask) {
				// For a glowmask that is the same dimensions as the item sprite, we can use all the provided parameters as is to draw the glowmask texture.
				// Instead of the provided lightColor, however, we use Color.White since we want to draw the glowmask texture at full brightness regardless of world lighting conditions.
				spriteBatch.Draw(frontTexture.Value, drawPosition, itemFrame, Color.White, rotation, origin, scale, SpriteEffects.None, 0);
			}
		}
	}
}
