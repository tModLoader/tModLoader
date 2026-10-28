using Terraria;

public class PlayerSelectedItemTest
{
	void Method(Player player) {
		int slot = player.selectedItem;
		player.selectedItemState.Select(3);
		player.selectedItemState.Select(slot + 1);
#if COMPILE_ERROR
		player.selectedItem++/* tModPorter Suggestion: selectedItem is read-only, use selectedItemState.Select(...) */;
#endif
		Main.LocalPlayer.selectedItemState.Select(player.selectedItem);
	}
}