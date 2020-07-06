using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class HeroThree : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Hero");
			Description.SetDefault("You are a hero of Terraria! (3 Lives)");
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
			canBeCleared = false;
		}
	}
}
