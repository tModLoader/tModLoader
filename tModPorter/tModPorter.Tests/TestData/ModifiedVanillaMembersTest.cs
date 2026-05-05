using Terraria;

public class ModifiedVanillaMembersTest
{
	void Method() {
		Chest.FindChestByGuessing(100, 200);

		var player = new Player();
		int talkNPC;
		player.talkNPC = 1; // set changed
		talkNPC = player.talkNPC; // get unchanged

		player.Spawn();

		player.SporeSac();

		ItemText.NewText(null, 0, false, false);

		var height = Main.NPCAddHeight(123);
	}
}
