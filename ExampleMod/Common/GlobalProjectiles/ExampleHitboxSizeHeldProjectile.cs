using ExampleMod.Common.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalProjectiles
{
	// Held projectiles, like shortswords, drills and some bows, are drawn along with the player holding them, but from their own position.
	// This means ExampleHitboxSizePlayer.TransformDrawData doesn't scale them, so they'd be drawn the default size, next to where a default sized player's hand would be.
	// This scales them around the same point as the player while they're drawn, then puts them back, so their hitbox and AI aren't affected.
	public class ExampleHitboxSizeHeldProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private bool scaledForDrawing;
		private Vector2 positionBeforeDrawing;
		private float scaleBeforeDrawing;

		public override bool PreDraw(Projectile projectile, Player player, ref Color lightColor) {
			// player is the one this projectile is drawn with, which isn't always its owner, such as for Mannequins.
			if (player.heldProj != projectile.whoAmI || !player.GetModPlayer<ExampleHitboxSizePlayer>().IsGrown) {
				return true;
			}

			scaledForDrawing = true;
			positionBeforeDrawing = projectile.position;
			scaleBeforeDrawing = projectile.scale;

			Vector2 origin = player.GetModPlayer<ExampleHitboxSizePlayer>().GetScaleOrigin(player.position);
			projectile.Center = origin + (projectile.Center - origin) * ExampleHitboxSizePlayer.GrownScale;
			projectile.scale *= ExampleHitboxSizePlayer.GrownScale;
			return true;
		}

		// PostDraw is always called after PreDraw, even if another mod's PreDraw returns false.
		public override void PostDraw(Projectile projectile, Player player, Color lightColor) {
			if (!scaledForDrawing) {
				return;
			}

			scaledForDrawing = false;
			projectile.position = positionBeforeDrawing;
			projectile.scale = scaleBeforeDrawing;
		}
	}
}
