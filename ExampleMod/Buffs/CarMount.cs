using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class CarMount : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Car");
			Description.SetDefault("Leather seats, 4 cupholders");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(mod.MountType<Mounts.Car>(), player);
			player.buffTime[buffIndex] = 10;
		}
	}
}
