using Terraria;

public class ModifiedVanillaMembersTest
{
	void Method() {
		var player = new Player();
		player.SetTalkNPC(1); // set changed
		int talkNPC = player.talkNPC; // get unchanged

#if COMPILE_ERROR
		player.Spawn(/* Suggestion: PlayerSpawnContext.SpawningIntoWorld */);

		PopupText.NewText(/* Suggestion: PopupTextContext.RegularItemPickup */, null, 0, false, false);
#endif
	}
}
