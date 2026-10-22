using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This buff demonstrates how to create a "crowd control" debuff, similar to Frozen, Webbed, or Stoned.
	/// The central focus of this example is calling player.SetCCed, this will affect the value of player.CCed, which is checked in many places and drives logic checking if the player is "crowd controlled" or not.
	/// </summary>
	public class ExampleCrowdControlledDebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			// SetCCed causes Player.CCed to be true. This indicates that the player is currently "crowd controlled" and Terraria and modded logic can use this to prevent certain actions while in this state.
			player.SetCCed();

			// We set ExampleCrowdControlledDebuffPlayer.exampleCrowdControlledDebuff to true to handle additional logic.
			player.GetModPlayer<ExampleCrowdControlledDebuffPlayer>().exampleCrowdControlledDebuff = true;

			// Set all player controls as unpressed to prevent movement.
			player.controlJump = false;
			player.controlDown = false;
			player.controlLeft = false;
			player.controlRight = false;
			player.controlUp = false;
			player.controlUseItem = false;
			player.controlUseTile = false;
			player.controlThrow = false;
			player.gravDir = 1f;
		}
	}

	/// <summary>
	/// We use this class to implement additional logic for this debuff that would not be possible in ExampleCrowdControlledDebuff.Update directly.
	/// Many things are handled automatically by player.SetCCed/player.CCed, but some things, like dismounting from mounts and ropes, are left to the modder to implement to customize if the debuff should do that or not.
	/// </summary>
	public class ExampleCrowdControlledDebuffPlayer : ModPlayer
	{
		public bool exampleCrowdControlledDebuff;

		public override void ResetEffects() {
			exampleCrowdControlledDebuff = false;
		}

		public override void PostUpdateMiscEffects() {
			if (!exampleCrowdControlledDebuff)
				return;

			Player.pulley = false;

			if (Player.mount.Active) {
				Player.mount.Dismount(Player);
			}
		}

		// DrawEffects is used to provide a simple visual effect, tinting the player blue (by darkening green and red).
		// A more complete implementation would likely use a custom PlayerDrawLayer to draw over the player, like how Frozen draws Ice over the player.
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			if (exampleCrowdControlledDebuff) {
				g *= 0.2f;
				r *= 0.2f;
			}
		}
	}
}
