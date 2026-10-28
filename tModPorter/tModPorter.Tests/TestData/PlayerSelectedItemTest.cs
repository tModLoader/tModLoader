using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		player.selectedItem = slot;
	}

	void UseItemInSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItem = slot;
		player.ItemCheck();
		player.selectedItem = previous;
	}

	void IncrementSlot(Player player) {
		player.selectedItem++;
	}
}
