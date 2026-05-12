using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This buff demonstrates how to create debuffs with a Frozen like effect
	/// </summary>
	public class ExampleFrozenDebuff : ModBuff
	{
		// For the sake of convenience, the example uses the name and description of vanilla
		public override LocalizedText DisplayName => Language.GetText("BuffName.Frozen");
		public override LocalizedText Description => Language.GetText("BuffDescription.Frozen");

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			// If a buff similar to Frozen is written, this method should be called inside the buff to mark CCed as true
			player.SetCCed();
			player.GetModPlayer<ExampleFrozenDebuffPlayer>().exampleFrozenDebuff = true;
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
	/// ModPlayer class for implementing Frozen effects
	/// </summary>
	public class ExampleFrozenDebuffPlayer : ModPlayer
	{
		public bool exampleFrozenDebuff;
		public override void ResetEffects() {
			exampleFrozenDebuff = false;
		}
		public override void PostUpdateMiscEffects() {
			if (!exampleFrozenDebuff)
				return;

			Player.pulley = false;
		}
		public override bool CanUseItem(Item item) {
			return !exampleFrozenDebuff;
		}
		public override bool CanStartExtraJump(ExtraJump jump) {
			return !exampleFrozenDebuff;
		}
	}
}
