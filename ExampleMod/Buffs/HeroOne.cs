using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class HeroOne : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Hero");
			Description.SetDefault("You are a hero of Terraria!");
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
			canBeCleared = false;
		}
	}
}
