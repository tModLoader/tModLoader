using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class ExampleMinecartBuff : ModBuff //this buff serves the smae purpose as for car.cs or any minion - disabling the buff disables the mount
	{ 
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Example Minecart");
			Description.SetDefault("Example your rails");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(ModContent.MountType<Mounts.ExampleMinecart>(), player);
			player.buffTime[buffIndex] = 10;
		}
	}
}