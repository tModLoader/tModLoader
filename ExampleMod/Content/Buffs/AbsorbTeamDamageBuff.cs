using ExampleMod.Common.Players;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class AbsorbTeamDamageBuff : ModBuff
	{
		public static readonly int TeamDamageAbsorptionPercent = 30;
		public static float TeamDamageAbsorptionMultiplier => TeamDamageAbsorptionPercent / 100f;

		public override LocalizedText Description => base.Description.WithFormatArgs(TeamDamageAbsorptionPercent);

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleDamageModificationPlayer>().defendedByAbsorbTeamDamageEffect = true;
		}
	}
}
