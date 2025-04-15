using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace ExampleMod.Content.CustomModType
{
	// An advanced example of a ModVictoryPose.
	public class HandsUpWithFireworksVictoryPose : ModVictoryPose
	{
		public override void SetStaticDefaults() {
			PoseTime = 180;
		}

		public override void OnStartPose(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Main.blockMouse = true;
			}
		}

		public override void Update(Player player) {
			if (player.itemAnimation == 0) {
				player.bodyFrame.Y = player.bodyFrame.Height * 5; // 2 Hands up / falling
			}

			float elapsedPoseTime = ElapsedPoseTime(player);
			if (elapsedPoseTime == 30 || elapsedPoseTime == 60) {
				SpawnFirework(player);
				SoundEngine.PlaySound(SoundID.Thunder with { Type = SoundType.Sound }, player.Center);
			}
		}

		public override void OnEndPose(Player player) {
			for (int i = 0; i < 3; i++) {
				SpawnFirework(player);
			}
		}

		private void SpawnFirework(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}

			int fireworkProjectile = ProjectileID.RocketFireworksBoxRed + Main.rand.Next(4);
			Projectile.NewProjectile(player.GetSource_FromThis(), player.Top, new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), fireworkProjectile, 0, 0, Main.myPlayer);
		}
	}
}
