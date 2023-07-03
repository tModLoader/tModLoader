using Terraria.ModLoader;
using ExampleMod.Content.Mounts;
using Terraria.Localization;
using Terraria;
using ExampleMod.Content.Items.Mounts;

namespace ExampleMod.Content.Buffs
{
	//TODO 1.4.5: review MinecartLeft/Right if it still exists
	public class ExampleMinecartBuff : ModBuff
	{
		// Use the vanilla DisplayName ("Minecart")
		//public override LocalizedText DisplayName => Language.GetText($"BuffName.MinecartLeft");
		// But for the sake of example, we want to reuse the item name
		public override LocalizedText DisplayName => ModContent.GetInstance<ExampleMinecart>().DisplayName;

		// Use the vanilla Description
		public override LocalizedText Description => Language.GetText($"BuffDescription.MinecartLeft");

		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			//TODO 1.4.5: review new Set for buff -> mount mapping, ExampleMountBuff too
			player.mount.SetMount(ModContent.MountType<ExampleMinecartMount>(), player);
			player.buffTime[buffIndex] = 10;
		}
	}
}
