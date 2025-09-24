using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	// This Dust will show off Dust.customData, using vanilla dust texture, and some neat movement.
	internal class ExampleAdvancedDust : ModDust
	{
		/*
			Spawning this dust is a little more involved because we need to assign a rotation, customData, and fix the position.
			Position must be fixed here because otherwise the first time the dust is drawn it'll draw in the incorrect place.
			This dust is not used in ExampleMod yet, so you'll have to add some code somewhere. Try ExamplePlayer.DrawEffects.

			Dust dust = Dust.NewDustDirect(Player.Center, 0, 0, ModContent.DustType<Content.Dusts.AdvancedDust>(), Scale: 2);
			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = Player;
			dust.position = Player.Center + Vector2.UnitX.RotatedBy(dust.rotation, Vector2.Zero) * dust.scale * 50;
		*/
		public override string Texture => null; // If we want to use vanilla texture

		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;

			// Since the vanilla dust texture has all the dust in 1 file, we'll need to do some math.
			// If you want to use a vanilla dust texture, you can copy and paste it, changing the desiredVanillaDustTexture
			int desiredVanillaDustTexture = DustID.Confetti;
			int frameX = desiredVanillaDustTexture * 10 % 1000;
			int frameY = desiredVanillaDustTexture * 10 / 1000 * 30 + Main.rand.Next(3) * 10;
			dust.frame = new Rectangle(frameX, frameY, 8, 8);

			dust.velocity = Vector2.Zero;
		}

		// This Update method shows off some interesting movement. Using customData assigned to a Player, we spiral around the Player while slowly getting closer. In practice, it looks like a vortex.
		public override bool Update(Dust dust) {
			// Here we rotate and scale down the dust. The dustIndex % 2 == 0 part lets half the dust rotate clockwise and the other half counter clockwise
			dust.rotation += 0.1f * (dust.dustIndex % 2 == 0 ? -1 : 1);
			dust.scale -= 0.05f;

			// Here we use the customData field. If customData is the type we expect, Player, we do some special movement.
			if (dust.customData != null && dust.customData is Player player) {
				// Here we assign position to some offset from the player that was assigned. This offset scales with dust.scale. The scale and rotation cause the spiral movement we desired.
				dust.position = player.Center + Vector2.UnitX.RotatedBy(dust.rotation, Vector2.Zero) * dust.scale * 50;
			}

			// Here we make sure to kill any dust that get really small.
			if (dust.scale < 0.25f)
				dust.active = false;

			return false;
		}
	}
}
