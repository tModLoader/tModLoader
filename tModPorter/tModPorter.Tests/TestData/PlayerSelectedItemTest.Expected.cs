using Terraria;

public class PlayerSelectedItemTest
{
	void SetSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItemState.Select(3)/* tModPorter Suggestion: Select is buffered for the local player, selectedItem and HeldItem keep the old value until the next player update */;
		player.selectedItemState.Select(slot + 1);
	}

	void CopySlot(Player player) {
		Main.LocalPlayer.selectedItemState.Select(player.selectedItem);
	}

	void UseItemInSlot(Player player, int slot) {
		int previous = player.selectedItem;
		player.selectedItemState.Select(slot)/* tModPorter Suggestion: Select is buffered for the local player, selectedItem and HeldItem keep the old value until the next player update */;
		player.ItemCheck();
		player.selectedItemState.Select(previous);
	}

	int HeldItemAfterSelect(Player player) {
		player.selectedItemState.Select(3)/* tModPorter Suggestion: Select is buffered for the local player, selectedItem and HeldItem keep the old value until the next player update */;
		return player.HeldItem.type;
	}

	void IncrementSlot(Player player) {
#if COMPILE_ERROR
		player.selectedItem++/* tModPorter Suggestion: selectedItem is read-only, use selectedItemState.Select(...) */;
		player.selectedItem += 2/* tModPorter Suggestion: selectedItem is read-only, use selectedItemState.Select(...) */;
#endif
	}
}