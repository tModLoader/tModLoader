using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Makes the player bigger while ExampleHitboxSizeAccessory is equipped: their hitbox through ModifyHitboxSize, and how they're drawn through TransformDrawData.
	// Every client works this out for itself from the equipped accessory and the tiles around the player, so nothing extra needs to be synced.
	public class ExampleHitboxSizePlayer : ModPlayer
	{
		public const float GrownScale = 1.5f;

		public bool WantsToGrow;

		// Unlike WantsToGrow, this isn't reset every update, since the player can't always grow right away.
		public bool IsGrown;

		public override void ResetEffects() {
			WantsToGrow = false;
		}

		public override void PostUpdateEquips() {
			if (WantsToGrow == IsGrown) {
				return;
			}

			if (WantsToGrow) {
				// Always check that the bigger hitbox fits before growing, otherwise the player can end up stuck inside tiles.
				// The current hitbox already includes any mount or step stool height, so only the extra base size is added on top.
				Vector2 grownSize = Player.Size + Player.GetBaseHitboxSize() * (GrownScale - 1f);
				if (!Player.CanFitInSpaceWithSize(grownSize)) {
					return;
				}
			}

			// Shrinking back always fits, since the bottom center of the hitbox stays in place.
			IsGrown = WantsToGrow;
		}

		public override void ModifyHitboxSize(ref Vector2 size) {
			if (IsGrown) {
				size *= GrownScale;
			}
		}

		// The hitbox doesn't change how the player is drawn: they're drawn at the default size with their feet at the edge of their hitbox, along with their held item and mount. So scale the drawing to match.
		// This scales everything drawn as part of the player, including armor, held items and mounts, around their feet.
		// PlayerDrawLayers.DrawPlayer_ScaleDrawData isn't used, since it always scales around the bottom of the hitbox, which isn't where the feet are while gravity is reversed.
		// Held projectiles, like shortswords, are drawn separately, so ExampleHitboxSizeHeldProjectile scales those. Other projectiles, like yoyo strings and flails, aren't scaled.
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			if (!IsGrown) {
				return;
			}

			Vector2 origin = GetScaleOrigin(drawInfo.Position) - Main.screenPosition;
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData data = drawInfo.DrawDataCache[i];
				data.position = origin + (data.position - origin) * GrownScale;
				data.scale *= GrownScale;
				drawInfo.DrawDataCache[i] = data;
			}
		}

		// The point the player's drawing is scaled around: the bottom center of the hitbox, or the top center while gravity is reversed, since that's where the feet are.
		// position is the top left of the hitbox. It's passed in since the player isn't always drawn at Player.position, such as in the character select menu.
		public Vector2 GetScaleOrigin(Vector2 position) {
			return position + Player.Size * new Vector2(0.5f, Player.gravDir == 1f ? 1f : 0f);
		}
	}
}
