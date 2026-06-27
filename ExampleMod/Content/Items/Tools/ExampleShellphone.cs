using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleShellphone : ModItem
	{
		public override void SetStaticDefaults() {
			// After right clicking Shellphone(Model) in inventory, it will become Shellphone(Ocean).
			ItemID.Sets.RightClickItemSwap[Type] = ItemID.ShellphoneOcean;

			// After right clicking Shellphone(Home) in inventory, it will become Shellphone(Model).
			ItemID.Sets.RightClickItemSwap[ItemID.Shellphone] = Type;

			// Make the sound when changing is SoundID.Unlock that is the same as other Shellphone.
			ItemID.Sets.UseUnlockSoundStyleAfterItemSwap[Type] = true;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Shellphone);
		}

		public override void UpdateInfoAccessory(Player player) {
			// Displays all infos
			player.accWatch = 3;
			player.accDepthMeter = 1;
			player.accCompass = 1;
			player.accFishFinder = true;
			player.accWeatherRadio = true;
			player.accCalendar = true;
			player.accThirdEye = true;
			player.accJarOfSouls = true;
			player.accCritterGuide = true;
			player.accStopwatch = true;
			player.accOreFinder = true;
			player.accDreamCatcher = true;
		}
	}
}
