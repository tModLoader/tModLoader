using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItem = 3;
		player.selectedItem = slot + 1;
	}

	void CopySlot(Player player) {
		Main.LocalPlayer.selectedItem = player.selectedItem;
	}

	void UseItemInSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItem = slot;
		player.ItemCheck();
		player.selectedItem = previous;
	}

	int HeldItemAfterSelect(Player player) {
		player.selectedItem = 3;
		return player.HeldItem.type;
	}

	void IncrementSlot(Player player) {
		player.selectedItem++;
		player.selectedItem += 2;
	}
}
