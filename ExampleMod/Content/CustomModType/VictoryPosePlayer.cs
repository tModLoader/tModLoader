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
	/// <para/> It also handles the default trigger for starting a pose, defeating an enemy. When a regular enemy is defeated, there is a chance that the player will start a pose. For bosses it is guaranteed.
	/// <para/> Other mods can use the API exposed in <see cref="VictoryPoseLoader"/> to implement their own systems for triggering a pose.
	/// </summary>
	internal class VictoryPosePlayer : ModPlayer
	{
		/// <summary> The pose that is currently playing. </summary>
		public ModVictoryPose activeVictoryPose;

		/// <summary> How long the active ModVictoryPose has been active. Will be -1 if there is no active pose. Can be used during <see cref="ModVictoryPose.Update(Player)"/> to drive effects. </summary>
		public int ElapsedPoseTime { get; internal set; }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// Detect when we defeat an enemy by doing the final hit.
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

			ModVictoryPose newPose;
			if (boss) {
				// Choose from all the poses
				newPose = Main.rand.Next(VictoryPoseLoader.victoryPoses);
			}
			else {
				// Choose from only the NonBoss set
				int randomNonBossVictoryPoseIndex = Main.rand.Next(VictoryPoseID.Sets.NonBoss.GetTrueIndexes());
				newPose = VictoryPoseLoader.victoryPoses[randomNonBossVictoryPoseIndex];
			}

			// Attempt to start the chosen pose. Since forced is false, we will not interrupt an ongoing pose.
			VictoryPoseLoader.StartPose(newPose, forced: false);
		}

		// Handles starting a pose, but does not do network sync, that needs to be done by calling code.
		internal bool StartPose(ModVictoryPose newPose, bool forced = false) {
			if (activeVictoryPose != null) {
				if (forced) {
					EndPose();
				}
				else {
					return false;
				}
			}

			ElapsedPoseTime = 0;
			activeVictoryPose = newPose;
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

			return true;
		}

		private void StartPoseDirect(ModVictoryPose newPose) {
			// The "direct" version of this method is intended for network scenarios.
			// Even if there is an activeVictoryPose, we will immediately end it since we can assume the pose from the network is more correct. (in cases of network desync or lag)
			StartPose(newPose, forced: true);
		}

		internal void EndPose() {
			activeVictoryPose?.OnEndPose(Player);
			activeVictoryPose = null;
			ElapsedPoseTime = -1;
		}

		public override void PostUpdate() {
			// Here we manage the lifetime of a victory pose
			if (activeVictoryPose == null) {
				return;
			}

			activeVictoryPose.Update(Player);
			ElapsedPoseTime++;
			if (ElapsedPoseTime >= activeVictoryPose.PoseTime) {
				EndPose();
			}
		}

		public static void HandleStartVictoryPoseMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				// This check forces the affected player to be whichever client sent the message to the server, this prevents other clients from spoofing a message for another player. This is a typical approach for untrusted messages from clients.
				player = whoAmI;
			}

			int poseIndex = reader.ReadInt32();
			ModVictoryPose pose = VictoryPoseLoader.victoryPoses[poseIndex];
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

		public static void HandleCancelVictoryPoseMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
			}

			if (player != Main.myPlayer) {
				Main.player[player].GetModPlayer<VictoryPosePlayer>().EndPose();
			}

			if (Main.netMode == NetmodeID.Server) {
				// If the server receives this message, it sends it to all other clients to sync the effects.
				SendCancelVictoryPoseMessage(player);
			}
		}

		public static void SendCancelVictoryPoseMessage(int whoAmI) {
			ModPacket packet = ModContent.GetInstance<ExampleMod>().GetPacket();
			packet.Write((byte)ExampleMod.MessageType.CancelVictoryPose);
			packet.Write((byte)whoAmI);
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
