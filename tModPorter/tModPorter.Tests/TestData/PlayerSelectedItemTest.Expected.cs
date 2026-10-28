using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		player.selectedItemState.Select(slot);
	}

	void UseItemInSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItemState.Select(slot)/* tModPorter Suggestion: Select is buffered, selectedItem won't change until the next update */;
		player.ItemCheck();
		player.selectedItemState.Select(previous);
	}

	void IncrementSlot(Player player) {
#if COMPILE_ERROR
		player.selectedItem++/* tModPorter Suggestion: selectedItem is read-only, use selectedItemState.Select(...) */;
#endif
	}
}