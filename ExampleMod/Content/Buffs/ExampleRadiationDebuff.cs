using Terraria;
using Terraria.ModLoader;
using System.Linq;

namespace ExampleMod.Content.Buffs
{
	public class ExampleRadiationDebuff : ModBuff
	{
		public override Player.RegenEffect MyRegenEffects => new Player.RegenEffect(
			// The name you want this effect to be internally known and searchable by in RegenEffect.effects. Recommend using full name
			FullName,

			// The struct of the life regen stats to apply associated with this debuff, used every frame update.
			Player.RegenEffect.ByStatStruct.Create(

				// Uses the available player instance, and returns if the debuff is active
				(player) => player.buffType.Contains(Type),

				// Does not use the available player instance, and returns an int corresponding to the damage the debuff does 
				_ => -(4 + Main.rand.Next(32)), // Does between 4 and 36 damage every 120 seconds 

				// In the case of regenerative buffs, set to true to allow it to create net positive regen despite a damaging debuff being present.
				false,

				// Vanilla required an additional effects field, so we get one too. Can apply additional effects based on this void delegate
				(player) => player.onFire = Main.rand.Next(100) <= 33
			),

			// The struct of the mana regen stats to apply associated with this debuff, used every frame update.
			Player.RegenEffect.ByStatStruct.nullStruct // Denotes that we are not applying any changes to mana regen stats
		); 

		public override void SetDefaults() {
			DisplayName.SetDefault("Radiated debuff");
			Description.SetDefault("The radiation is burning your lungs");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true; //Nurse removes the buff when healing
		}

		
	}
}