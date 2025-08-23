using ExampleMod.Content.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This example demonstrates an interactable projectile, similar to the Void Lens or Flying Piggy Bank (Money Trough)
	// Interactable projectiles can be right clicked to provide some effect. They also support smart cursor.
	// Vanilla interactable projectiles allow portable access to player-specific banks, but this in not yet supported for modded projectiles.
	// This example is a clone of the Flying Piggy Bank projectile except it simply plays sounds when interacted with instead of accessing a player bank.
	public class ExampleInteractableProjectile : ModProjectile
	{
		// An outline texture indicating the smart cursor selecting this projectile
		private static Asset<Texture2D> highlightTexture;

		public override void Load() {
			highlightTexture = ModContent.Request<Texture2D>(Texture + "_Highlight");
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsInteractable[Type] = true; // Facilitates smart cursor support
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true; // Necessary for non-held projectiles using Projectile.hide
			Main.projFrames[Type] = 5;
		}

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 10800; // Stays active for 3 minutes, or 3 * 60 * 60 game updates
			Projectile.hide = true;

			// Draw the projectile higher to line up the hitbox with the body of the projectile, not the flapping wings.
			DrawOriginOffsetY = -5;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindProjectiles.Add(index); // This projectile draws behind other projectiles to not be in the way.
		}

		public override void PostDraw(Color lightColor) {
			// We use PostDraw to draw the highlight texture over the normal texture.

			// This logic replicates the vanilla projectile drawing logic:
			Asset<Texture2D> texture = TextureAssets.Projectile[Type];
			int offsetY = 0;
			int offsetX = 0;
			float originX = (texture.Width() - Projectile.width) * 0.5f + Projectile.width * 0.5f;
			ProjectileLoader.DrawOffset(Projectile, ref offsetX, ref offsetY, ref originX);
			int frameHeight = texture.Height() / Main.projFrames[Type];
			int frameY = frameHeight * Projectile.frame;
			SpriteEffects drawEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// TryInteracting return values:
			// 0: Not highlighted, 1: draw faded highlight, 2: draw selected highlight selected
			int highlightTextureDrawMode = TryInteracting();
			if (highlightTextureDrawMode == 0) {
				// If not in range, or if smart cursor is off, we don't draw the highlight texture at all.
				return;
			}

			int lightValue = (lightColor.R + lightColor.G + lightColor.B) / 3;
			if (lightValue > 10) {
				bool isProjectileSelected = highlightTextureDrawMode == 2;
				Color selectionGlowColor = Colors.GetSelectionGlowColor(isProjectileSelected, lightValue);
				Main.EntitySpriteDraw(
					highlightTexture.Value,
					new Vector2(Projectile.position.X - Main.screenPosition.X + originX + offsetX, Projectile.position.Y - Main.screenPosition.Y + (Projectile.height / 2) + Projectile.gfxOffY),
					new Rectangle(0, frameY, texture.Width(), frameHeight - 1),
					selectionGlowColor,
					Projectile.rotation,
					new Vector2(originX, Projectile.height / 2 + offsetY),
					1f,
					drawEffects
				);
			}
		}

		// This method handles interacting with this projectile and also returns a value indicating how the highlight texture should be drawn.
		private int TryInteracting() {
			if (Main.gamePaused || Main.gameMenu) {
				return 0;
			}

			bool cursorHighlights = Main.SmartCursorIsUsed || PlayerInput.UsingGamepad;
			Player localPlayer = Main.LocalPlayer;
			Vector2 compareSpot = localPlayer.Center;
			if (!localPlayer.IsProjectileInteractibleAndInInteractionRange(Projectile, ref compareSpot)) {
				return 0;
			}

			// Due to a quirk in how projectiles drawn using behindProjectiles are implemented, we need to do some math to calculate the correct world position of the mouse instead of using Main.MouseWorld directly.
			Matrix matrix = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
			Vector2 position = Main.ReverseGravitySupport(Main.MouseScreen);
			Vector2.Transform(Main.screenPosition, matrix);
			Vector2 realMouseWorld = Vector2.Transform(position, matrix) + Main.screenPosition;

			bool mouseDirectlyOver = Projectile.Hitbox.Contains(realMouseWorld.ToPoint());
			bool interactingWithThisProjectile = mouseDirectlyOver || Main.SmartInteractProj == Projectile.whoAmI;
			if (!interactingWithThisProjectile || localPlayer.lastMouseInterface) {
				if (cursorHighlights) {
					return 1; // Draw faded highlight texture
				}
				else {
					return 0; // Don't draw highlight texture
				}
			}

			Main.HasInteractibleObjectThatIsNotATile = true;
			if (mouseDirectlyOver) {
				localPlayer.noThrow = 2;
				// Show the corresponding item icon on the cursor when directly over the interactable projectile.
				localPlayer.cursorItemIconEnabled = true;
				localPlayer.cursorItemIconID = ModContent.ItemType<ExampleInteractableProjectileItem>();
			}

			if (PlayerInput.UsingGamepad) {
				localPlayer.GamepadEnableGrappleCooldown();
			}
			if (Main.mouseRight && Main.mouseRightRelease && Player.BlockInteractionWithProjectiles == 0) {
				Main.mouseRightRelease = false;
				localPlayer.tileInteractAttempted = true;
				localPlayer.tileInteractionHappened = true;
				localPlayer.releaseUseTile = false;

				// This is where custom interaction logic would go. This example simply plays a sound.
				SoundEngine.PlaySound(Main.rand.NextBool(10) ? SoundID.Duck with { Type = SoundType.Sound } : SoundID.Item59);
			}

			if (cursorHighlights) {
				return 2; // Draw highlight texture
			}
			else {
				return 0;
			}
		}

		public override void AI() {
			// This animation goes 0, 1, 2, 3, 4, 3, 2, 1 and repeats.
			Projectile.frameCounter++;
			Projectile.frame = Projectile.frameCounter / 4; // 4 ticks per frame
			if (Projectile.frame >= 5) {
				Projectile.frame = 8 - Projectile.frame;
			}
			if (Projectile.frameCounter >= 8 * 4) {
				Projectile.frameCounter = 0;
			}

			// Let the game know to check for interactable projectiles
			Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

			// Replace older projectiles when a new one is spawned.
			if (Projectile.owner == Main.myPlayer) {
				for (int i = 0; i < 1000; i++) {
					Projectile otherProjectile = Main.projectile[i];
					if (i != Projectile.whoAmI && otherProjectile.active && otherProjectile.owner == Projectile.owner && otherProjectile.type == Projectile.type) {
						if (Projectile.timeLeft >= otherProjectile.timeLeft) {
							otherProjectile.Kill();
						}
						else {
							Projectile.Kill();
						}
					}
				}
			}

			// When initially spawned, the projectile slows down until it stops.
			if (Projectile.ai[0] == 0f) {
				if (Projectile.velocity.Length() < 0.1f) {
					// Once slowed down enough, stop completely and start bobbing
					Projectile.velocity.X = 0f;
					Projectile.velocity.Y = 0f;
					Projectile.ai[0] = 1f;
					Projectile.ai[1] = 45f;
					return;
				}

				// Deaccelerate while facing in the direction of travel.
				Projectile.velocity *= 0.94f;
				Projectile.direction = Projectile.velocity.X < 0f ? -1 : 1;
				Projectile.spriteDirection = Projectile.direction;
				return;
			}

			// Always face the player when stopped.
			Projectile.direction = Main.player[Projectile.owner].Center.X < Projectile.Center.X ? -1 : 1;
			Projectile.spriteDirection = Projectile.direction;

			// ai[1] is a timer that helps the projectile hover in-place while bobbing up and down slightly.
			Projectile.ai[1] += 1f;
			float acceleration = 0.005f;
			if (Projectile.ai[1] > 0f) {
				Projectile.velocity.Y -= acceleration;
			}
			else {
				Projectile.velocity.Y += acceleration;
			}
			if (Projectile.ai[1] >= 90f) {
				Projectile.ai[1] *= -1f;
			}
		}
	}
}
