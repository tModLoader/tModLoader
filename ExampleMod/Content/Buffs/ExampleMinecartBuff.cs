using ExampleMod.Content.Items.Mounts;
using ExampleMod.Content.Mounts;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	// TODO 1.4.5: review MinecartLeft/Right if it still exists
	public class ExampleMinecartBuff : ModBuff
	{
		// Use the vanilla DisplayName ("Minecart")
		//public override LocalizedText DisplayName => Language.GetText("BuffName.MinecartLeft");
		// But for the sake of example, we want to reuse the item name
		public override LocalizedText DisplayName => ModContent.GetInstance<ExampleMinecart>().DisplayName;

		// Use the vanilla Description
		public override LocalizedText Description => Language.GetText("BuffDescription.MinecartLeft");

		public override void SetStaticDefaults() {
			// Handles automatically mounting the player within Update, and setting Main.buffNoTimeDisplay/buffNoSave (no need to write yourself like in ExampleMountBuff)
			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = ModContent.MountType<ExampleMinecartMount>()
			};
		}
	}
}
