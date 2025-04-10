using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.Graphics.Renderers;
using Terraria.ID;
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
			if (!target.active && Player.whoAmI == Main.myPlayer) {
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

			ModVictoryPose newPose;
			if (boss) {
				// Choose from all the poses
				newPose = Main.rand.Next(VictoryPoseLoader.VictoryPoses);
			}
			else {
				// Choose from only the NonBoss set
				int randomNonBossVictoryPoseIndex = Main.rand.Next(VictoryPoseID.Sets.NonBoss.GetTrueIndexes());
				newPose = VictoryPoseLoader.VictoryPoses[randomNonBossVictoryPoseIndex];
			}
			StartPose(newPose);

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				// Inform other clients about the pose to sync the visuals.
				SendStartVictoryPoseMessage(Player.whoAmI, activeVictoryPose);
			}
		}

		private void StartPose(ModVictoryPose newPose) {
			activeVictoryPose = newPose.Clone();

			// Reset timers.
			activeVictoryPose.ElapsedPoseTime = 0;
			activeVictoryPose.OnStartPose(Player);

			if (Main.netMode != NetmodeID.Server) {
				ChatHelper.DisplayMessage(activeVictoryPose.VictoryCheer.ToNetworkText(), Color.White, (byte)Player.whoAmI);
			}

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(activeVictoryPose.Texture);
			Rectangle? frame = activeVictoryPose.GetTextureFrame(texture);
			Main.ParticleSystem_World_BehindPlayers.Add(new PoseIconParticle(texture, frame, new Vector2(Main.rand.NextFloat(-2, 2), -5f), Player.Center - new Vector2(0, 80), Main.rand.NextFloat(-0.03f, 0.03f)) {
				AccelerationPerFrame = new Vector2(0f, 0.16350001f),
				ScaleOffsetPerFrame = 1f / 60f,
			});
		}

		private void StartPoseDirect(ModVictoryPose newPose) {
			// The "direct" version of this method is intended for network scenarios.
			// Even if there is an activeVictoryPose, we will immediately end it since we can assume the pose from the network is more correct. (in cases of network desync or lag)
			activeVictoryPose?.OnEndPose(Player);
			StartPose(newPose);
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

		public static void HandleStartVictoryPoseMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				// This check forces the affected player to be whichever client sent the message to the server, this prevents other clients from spoofing a message for another player. This is a typical approach for untrusted messages from clients.
				player = whoAmI;
			}

			int poseIndex = reader.ReadInt32();
			ModVictoryPose pose = VictoryPoseLoader.VictoryPoses[poseIndex];
			if (player != Main.myPlayer) {
				Main.player[player].GetModPlayer<VictoryPosePlayer>().StartPoseDirect(pose);
			}

			if (Main.netMode == NetmodeID.Server) {
				// If the server receives this message, it sends it to all other clients to sync the effects.
				SendStartVictoryPoseMessage(player, pose);
			}
		}

		public static void SendStartVictoryPoseMessage(int whoAmI, ModVictoryPose pose) {
			ModPacket packet = ModContent.GetInstance<ExampleMod>().GetPacket();
			packet.Write((byte)ExampleMod.MessageType.StartVictoryPose);
			packet.Write((byte)whoAmI);
			packet.Write(pose.Type);
			packet.Send(ignoreClient: whoAmI);
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
