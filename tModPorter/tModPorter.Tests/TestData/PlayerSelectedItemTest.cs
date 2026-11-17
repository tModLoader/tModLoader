using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		player.selectedItem = slot;
	}

	void IncrementSlot(Player player) {
		player.selectedItem++;
	}
}
