using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Chat;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md

	/// <summary>
	/// This class handles applying and updating the active ModVictoryPose.
	/// <para/> When an enemy is defeated, there is a chance that the player will start a pose. For bosses it is guaranteed. 
	/// </summary>
	public class VictoryPosePlayer : ModPlayer
	{
		public ModVictoryPose activeVictoryPose;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// Detect when we defeat an enemy.
			if (!target.active) {
				// Start a victory pose
				StartRandomPose(target.boss);
			}
		}

		private void StartRandomPose(bool boss) {
			// Always pose after defeating a boss, pose 1 out of 100 times for normal enemies.
			if (!boss && Main.rand.NextBool(99, 100)) {
				return;
			}

			if (activeVictoryPose != null) {
				return; // Don't interrupt an ongoing pose
			}

			if (boss) {
				// Choose from all the poses
				activeVictoryPose = Main.rand.Next(VictoryPoseLoader.VictoryPoses);
			}
			else {
				// Choose from only the NonBoss set
				int randomNonBossVictoryPoseIndex = Main.rand.Next(VictoryPoseID.Sets.NonBoss.GetTrueIndexes());
				activeVictoryPose = VictoryPoseLoader.VictoryPoses[randomNonBossVictoryPoseIndex];
			}
			// Reset timers.
			activeVictoryPose.ElapsedPoseTime = 0;
			activeVictoryPose.OnStartPose(Player);

			ChatHelper.DisplayMessage(activeVictoryPose.VictoryCheer.ToNetworkText(), Color.White, (byte)Main.myPlayer);

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(activeVictoryPose.Texture);
			Rectangle? frame = activeVictoryPose.GetTextureFrame(texture);
			Main.ParticleSystem_World_BehindPlayers.Add(new PoseIconParticle(texture, frame, new Vector2(Main.rand.NextFloat(-2, 2), -5f), Player.Center - new Vector2(0, 80), Main.rand.NextFloat(-0.03f, 0.03f)) {
				AccelerationPerFrame = new Vector2(0f, 0.16350001f),
				ScaleOffsetPerFrame = 1f / 60f,
			});

			// TODO: A real implementation would want to sync the pose to other clients with a ModPacket
		}

		public override void PostUpdate() {
			// Here we manage the lifetime of a victory pose
			if (activeVictoryPose == null) {
				return;
			}

			activeVictoryPose.Update(Player);
			activeVictoryPose.ElapsedPoseTime++;
			if (activeVictoryPose.ElapsedPoseTime >= activeVictoryPose.PoseTime) {
				activeVictoryPose.OnEndPose(Player);
				activeVictoryPose = null;
			}
		}
	}

	// A slightly tweaked CreativeSacrificeParticle. IParticle are similar to Dust, but can be customized completely. 
	public class PoseIconParticle : IParticle
	{
		public Vector2 AccelerationPerFrame;
		public Vector2 Velocity;
		public float rotationRate;
		public Vector2 LocalPosition;
		public float ScaleOffsetPerFrame;
		public float StopWhenAboveXScale;
		private Asset<Texture2D> texture;
		private Rectangle frame;
		private Vector2 origin;
		private float scale;
		private float rotation;

		public bool ShouldBeRemovedFromRenderer { get; private set; }

		public PoseIconParticle(Asset<Texture2D> textureAsset, Rectangle? frame, Vector2 initialVelocity, Vector2 initialLocalPosition, float rotationRate) {
			texture = textureAsset;
			this.frame = frame ?? texture.Frame();
			origin = this.frame.Size() / 2f;
			Velocity = initialVelocity;
			LocalPosition = initialLocalPosition;
			StopWhenAboveXScale = 3f;
			ShouldBeRemovedFromRenderer = false;
			scale = 0.6f;
			this.rotationRate = rotationRate;
		}

		public void Update(ref ParticleRendererSettings settings) {
			Velocity += AccelerationPerFrame;
			LocalPosition += Velocity;
			scale += ScaleOffsetPerFrame;
			rotation += rotationRate;
			if (scale >= StopWhenAboveXScale) {
				ShouldBeRemovedFromRenderer = true;
			}
		}

		public void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
			Color color = Color.White * Utils.Clamp(scale, 0, 1f);
			spriteBatch.Draw(texture.Value, settings.AnchorPosition + LocalPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}
}
