using ExampleMod.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This buff is modeled after the "Holy Protection" buff given to the player by the Hallowed armor set bonus. <br/>
	/// Use <see cref="Items.Weapons.HitModifiersShowcase"/> in mode 7 to apply this buff.
	/// </summary>
	internal class ExampleDodgeBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleDamageModificationPlayer>().exampleDodge = true;
		}
	}
}
