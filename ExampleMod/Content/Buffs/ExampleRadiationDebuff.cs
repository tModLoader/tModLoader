using Terraria;
using Terraria.ModLoader;
using System.Linq;

namespace ExampleMod.Content.Buffs
{
	public class ExampleRadiationDebuff : ModBuff
	{
		public override Player.RegenEffect MyManaRegenEffects => new Player.RegenEffect(
			// The name you want this effect to be internally known and searchable by in RegenEffect.effects. Recommend using full name
			FullName,

			// The struct of the mana regen stat calculations to apply associated with this debuff, used every frame update.
			Player.RegenEffect.ByStatStruct.Create(

				// Uses the available player instance, and returns if the debuff is active
				(player) => player.buffType.Contains(Type),

				// Does not use the available player instance, and returns an float corresponding to the mana drain the debuff does 
				_ => -(0.5f + Main.rand.Next(32)), // Does between 0.5 and 32.5 drain every 120 seconds 

				// Set to true to allow your entry to have net positive regen despite a damaging debuff being present.
				false,

				// Can apply additional effects based on this void delegate, which receives parameters player and the delta calculated up to this point.
				(player, delta) => { player.onFire = Main.rand.Next(100) <= 33; delta *= (85 + Main.rand.Next(30)) / 100; } //Sets player on fire each frame at a 1/3 chance, and randomly modifies the drain in range of +/- 15%.
			)
		);

		public override void SetDefaults() {
			DisplayName.SetDefault("Radiated debuff");
			Description.SetDefault("The radiation is burning your lungs");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true; //Nurse removes the buff when healing
		}
	}
}