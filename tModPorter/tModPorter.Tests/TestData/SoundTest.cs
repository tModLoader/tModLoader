using Terraria;
using Terraria.ID;
using Terraria.Audio;
// Blank

public class SoundTest
{
	void Method() {
		Player player = new Player();

		Main.PlaySound(SoundID.MenuTick, player.position);

		Main.PlaySound(SoundID.MenuTick);
		Main.PlaySound(SoundID.MenuTick, -1, -1);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0, 1f);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0, 1f, 0f);

		Main.PlaySound(SoundID.MenuTick, 420, 421);
		Main.PlaySound(SoundID.MenuTick, (int)player.position.X, (int)player.position.Y);
		Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 17);

		Main.PlaySound(new LegacySoundStyle(2, 10), player.position);
		Main.PlaySound(new LegacySoundStyle(3, 10), player.position);
		Main.PlaySound(new LegacySoundStyle(4, 10), player.position);
		Main.PlaySound(new LegacySoundStyle(29, 10), player.position);

		Main.PlaySound(2, player.position, 10);
		Main.PlaySound(3, player.position, 10);
		Main.PlaySound(4, player.position, 10);
		Main.PlaySound(29, player.position, 10);

		Main.PlaySound(SoundID.Item, player.position, 10);
		Main.PlaySound(SoundID.NPCHit, player.position, 10);
		Main.PlaySound(SoundID.NPCKilled, player.position, 10);
		Main.PlaySound(SoundID.Zombie, player.position, 10);
	}
}