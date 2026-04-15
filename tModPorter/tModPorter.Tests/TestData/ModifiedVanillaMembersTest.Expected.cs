using Terraria;

public class ModifiedVanillaMembersTest
{
	void Method() {
		Chest.FindChestByGuessing(100, 200)/* tModPorter Note: Removed. Use Chest.FindChest with the top left tile coordinate */;

		// not-yet-implemented
		var player = new Player();
		player.SetTalkNPC(1); // set changed
		int talkNPC = player.talkNPC; // get unchanged

#if COMPILE_ERROR
		player.Spawn(/* tModPorter Suggestion: PlayerSpawnContext.SpawningIntoWorld */);

		player.SporeSac(/* tModPorter Suggestion: use the item object that causes this method to get called */);

		PopupText.NewText(/* tModPorter Suggestion: PopupTextContext.RegularItemPickup */, null, 0, false, false);

		var height = Main.NPCAddHeight(123/* tModPorter Suggestion: use the NPC object instead of the NPC type */);
#endif
		// instead-expect
		var player = new Player();
		player.talkNPC = 1; // set changed
		int talkNPC = player.talkNPC; // get unchanged

		player.Spawn();

		player.SporeSac();

		ItemText.NewText(null, 0, false, false);

		var height = Main.NPCAddHeight(123);
	}
}
