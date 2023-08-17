using Terraria;

public class ModifiedVanillaMembersTest
{
	void Method() {
		var player = new Player();
		player.talkNPC = 1; // set changed
		int talkNPC = player.talkNPC; // get unchanged

		player.Spawn();

		player.SporeSac();

		ItemText.NewText(null, 0, false, false);

		var height = Main.NPCAddHeight(123);
	}
}
