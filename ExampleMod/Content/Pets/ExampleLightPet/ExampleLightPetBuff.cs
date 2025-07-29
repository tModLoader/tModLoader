using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.ExampleLightPet
{
	public class ExampleLightPetBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			bool unused = false;
			player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<ExampleLightPetProjectile>());
		}
	}
}