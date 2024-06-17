using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// Only a single weapon imbue buff can apply to a player at a time, Main.meleeBuff ensures that this restriction is met.
	/// See also ExampleFlask and ExampleWeaponEnchantmentPlayer.
	/// </summary>
	public class ExampleWeaponImbue : ModBuff
	{
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAFlaskBuff[Type] = true;
			Main.meleeBuff[Type] = true;
			Main.persistentBuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleWeaponEnchantmentPlayer>().exampleWeaponImbue = true;

			// We set this to a value not used by existing imbues simply to indicate to other mods that a weapon imbue is active. The real flag is exampleWeaponImbue above.
			player.meleeEnchant = 255;
		}
	}
}
