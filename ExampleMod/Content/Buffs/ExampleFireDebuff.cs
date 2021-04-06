using ExampleMod.Common.Players;
using ExampleMod.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	// Fire debuff is an example of a buff that causes constant loss of life.
	// See ExamplePlayer.UpdateBadLifeRegen for more information.
	public class ExampleFireDebuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Fire debuff"); // Buff display name
			Description.SetDefault("Losing life"); // Buff description
			Main.debuff[Type] = true;  // Is it a debuff?
			Main.pvpBuff[Type] = true; // Players can give other players buffs, which are listed as pvpBuff
			Main.buffNoSave[Type] = true; // It means the buff won't save when you exit the world
			LongerExpertDebuff = true; // If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode
		}

		// Allows you to make this buff give certain effects to the given player
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExamplePlayer>().FireDebuff = true;
		}
	}
}
