using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		player.selectedItemState.Select(slot)/* tModPorter Note: Select is buffered, selectedItem won't change until the next update */;
	}

	void IncrementSlot(Player player) {
#if COMPILE_ERROR
		player.selectedItem++/* tModPorter Suggestion: selectedItem is read-only, use selectedItemState.Select(...) */;
#endif
	}
}