using ExampleMod.Common.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// By default, wings only support 4 frames of animation. This example shows using ModifyEquipTextureDraw and WingUpdate to implement a wing with 7 frames of animation. This example roughly clones the Grox The Great's Wings item effects.
	[AutoloadEquip(EquipType.Wings)]
	public class ExampleCustomDrawWings : ModItem
	{
		private static Asset<Texture2D> glowTexture;

		public override void Load() {
			glowTexture = ModContent.Request<Texture2D>(Texture + "_Wings_Glow");
		}

		public override void SetStaticDefaults() {
			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // Falling glide speed
			ascentWhenRising = 0.15f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.SortBefore(Main.recipe.First(recipe => recipe.createItem.wingSlot != -1)) // Places this recipe before any wing so every wing stays together in the crafting menu.
				.Register();
		}

		public override bool ModifyEquipTextureDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, EquipType type, int slot, string memberName) {
			// Some wings only draw while falling or being used, the ShouldDrawWingsThatAreAlwaysAnimated check does that.
			//if (!drawInfo.drawPlayer.ShouldDrawWingsThatAreAlwaysAnimated()) {
			//	return false; // false prevents drawData from being drawn
			//}

			// Since the normal wing logic assumes 4 frames of animation, and to adjust the placement of our wings on the player's back, we need to recalculate the DrawData values:
			Vector2 playerBackPosition = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, 7f);
			int wingFrameCount = 7;
			var texture = drawData.texture;
			Vector2 drawPosition = playerBackPosition + new Vector2(-14, -5) * drawInfo.drawPlayer.Directions;
			drawData = new DrawData(texture, drawPosition.Floor(), new Rectangle(0, texture.Height / wingFrameCount * drawInfo.drawPlayer.wingFrame, texture.Width, texture.Height / wingFrameCount), drawData.color, drawInfo.drawPlayer.bodyRotation, new Vector2(texture.Width / 2, texture.Height / wingFrameCount / 2), 1f, drawInfo.playerEffect);
			drawData.shader = drawInfo.cWings;

			// We can implement a glow mask by drawing the same drawData but with a new Texture and in a specific bright color. Here we manually add drawData to DrawDataCache and then add our glow mask DrawData. We need to do this to draw the glow mask in front of the normal texture.
			drawInfo.DrawDataCache.Add(drawData);
			for (int i = 0; i < 2; i++) {
				// Draw the glow mask twice, giving it a flame flicker effect.
				drawInfo.DrawDataCache.Add(drawData with { color = Color.White, texture = glowTexture.Value, position = drawPosition + Main.rand.NextVector2Circular(1.25f, 1.25f)});
			}

			// Return false to stop drawData from being added to DrawDataCache, since we already did that above.
			return false;
		}

		public override bool WingUpdate(Player player, bool inUse) {
			if (inUse || player.jump > 0) {
				player.wingFrameCounter++;
				if (player.wingFrameCounter > 3) {
					player.wingFrame++;
					player.wingFrameCounter = 0;
					if (player.wingFrame >= 7) {
						player.wingFrame = 1;
					}
				}
			}
			else if (player.velocity.Y != 0f) {
				player.wingFrame = 2;
				if (player.ShouldFloatInWater && player.wet) {
					player.wingFrame = 0;
				}
			}
			else {
				player.wingFrame = 0;
			}

			// Gliding - Attempting to fly without any remaining flight time. Not falling.
			if (!inUse && player.wingsLogic > 0 && player.controlJump && player.velocity.Y > 0f) {
				player.wingFrame = 1;

			}

			// Dust - spawn when flying or gliding at a reduced rate
			if (inUse) {
				if (Main.rand.NextBool(2)) {
					SpawnWingDust(player);
				}
			}
			else if (player.controlJump && player.velocity.Y > 0f) {
				if (Main.rand.NextBool(4)) {
					SpawnWingDust(player);
				}
			}

			// Flap sounds
			if (player.wingFrame == 4) {
				if (!player.flapSound) {
					SoundEngine.PlaySound(SoundID.Item32, player.position);
				}
				player.flapSound = true;
			}
			else {
				player.flapSound = false;
			}

			// Returning true to skip vanilla animations and sounds
			return true;
		}

		private void SpawnWingDust(Player player) {
			bool noLightEmittance = player.wingsLogic != player.wings; // Avoids visual wings from providing a functional effect by lighting the surroundings.
			int spawnXOffset = 4;
			if (player.direction == 1) {
				spawnXOffset = -40;
			}

			Dust dust = Dust.NewDustDirect(new Vector2(player.position.X + (player.width / 2) + spawnXOffset, player.position.Y + (player.height / 2) - 15f), 30, 30, DustID.AncientLight, 0f, 0f, 50, default, 0.6f);
			dust.fadeIn = 1.1f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.noLightEmittence = noLightEmittance;
			dust.velocity *= 0.3f;
			dust.shader = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
		}
	}
}
