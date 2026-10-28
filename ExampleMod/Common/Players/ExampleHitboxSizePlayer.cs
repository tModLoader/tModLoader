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

		// The hitbox doesn't change how the player is drawn, so scale the drawing to match.
		// This scales everything drawn as part of the player, including armor and held items, around the bottom center of the hitbox.
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			if (IsGrown) {
				PlayerDrawLayers.DrawPlayer_ScaleDrawData(ref drawInfo, GrownScale);
			}
		}
	}
}
