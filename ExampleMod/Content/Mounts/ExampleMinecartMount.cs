using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Mounts
{
	public class ExampleMinecartMount : ModMount
	{
		public override void SetStaticDefaults() {
			MountID.Sets.Cart[Type] = true;
			MountID.Sets.FacePlayersVelocity[Type] = true;

			// Helper method setting many common properties for a minecart
			Mount.SetAsMinecart(
				MountData,
				ModContent.BuffType<ExampleMinecartBuff>(),
				MountData.frontTexture
			);

			// Change properties on MountData here further, for example:
			MountData.spawnDust = 21;
			MountData.delegations.MinecartDust = DelegateMethods.Minecart.SparksMeow;
			MountData.delegations.MinecartLandingSound = DelegateMethods.Minecart.LandingSoundFart;
			MountData.delegations.MinecartBumperSound = DelegateMethods.Minecart.BumperSoundFart;

			// Note that runSpeed, dashSpeed, acceleration, jumpHeight, and jumpSpeed will be overridden when the player has used the Minecart Upgrade Kit.
			// To customize the Minecart Upgrade Kit stats, assign values to the MinecartUpgradeX fields:
			MountData.MinecartUpgradeRunSpeed = 40f;
			MountData.MinecartUpgradeDashSpeed = 40f;
			MountData.MinecartUpgradeAcceleration = 0.2f;
		}

		public override void UpdateEffects(Player player) {
			// Visuals copied from Diamond Minecart
			if (Main.rand.NextBool(10)) {
				Vector2 randomOffset = Main.rand.NextVector2Square(-1f, 1f) * new Vector2(22f, 10f);
				Vector2 directionOffset = new Vector2(0f, 10f) * player.Directions;
				Vector2 position = player.Center + directionOffset + randomOffset;
				position = player.RotatedRelativePoint(position);
				Dust dust = Dust.NewDustPerfect(position, DustID.GemDiamond);
				dust.noGravity = true;
				dust.fadeIn = 0.6f;
				dust.scale = 0.4f;
				dust.velocity *= 0.25f;
				dust.shader = GameShaders.Armor.GetSecondaryShader(player.cMinecart, player);
			}
		}
	}
}
