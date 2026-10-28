using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Makes the player's hitbox bigger while ExampleHitboxSizeAccessory is equipped.
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
	}
}
