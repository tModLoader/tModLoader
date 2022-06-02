using Terraria;

public class ModifiedVanillaMembersTest
{
	void Method() {
		var player = new Player();
		player.talkNPC = 1; // set changed
		int talkNPC = player.talkNPC; // get unchanged

#if COMPILE_ERROR
		player.Spawn();

		ItemText.NewText(null, 0, false, false);
#endif
	}
}
