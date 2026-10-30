using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.CreativePowers
{
	// A ModCreativeButton is a Journey Mode power that does something each time it's clicked, like vanilla's Change Time to Dawn.
	// This one goes on vanilla's Personal strip, after God Mode and the other personal powers. Time and Weather work the same way.
	public class ExampleRestoreManaButton : ModCreativeButton
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.ManaPotion}";

		public override int Category => CreativeCategoryID.Personal;

		public override void OnClick() {
			Player player = Main.LocalPlayer;
			player.ManaEffect(player.statManaMax2 - player.statMana);
			player.statMana = player.statManaMax2;
		}
	}
}
