using Terraria;

public class PlayerSelectedItemTest
{
	void Method(Player player) {
		int slot = player.selectedItem;
		player.selectedItem = 3;
		player.selectedItem = slot + 1;
		player.selectedItem++;
		Main.LocalPlayer.selectedItem = player.selectedItem;
	}
}
