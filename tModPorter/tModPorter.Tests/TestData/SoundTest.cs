using Terraria;
using Terraria.ID;
using Terraria.Audio;

//TODO modded sound retrieval&playback
//TODO non-dedicated IDs (Compile_Error req?)
//TODO pitch + volume
public class SoundTest
{
	void Method() {
		Player player = new Player();

		Main.PlaySound(SoundID.MenuTick, player.position); // This is the most straightforward overload to port: SoundID.Something which usually has an equivalent in 1.4s SoundID. SoundEngine requires Terraria.Audio, SoundID.MenuTick is SoundStyle and not int like 1.3

		Main.PlaySound(SoundID.MenuTick); // The following are different, using the most generic overload with all parameters
		Main.PlaySound(SoundID.MenuTick, -1, -1);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0, 1f);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0, 1f, 0f);

		Main.PlaySound(SoundID.MenuTick, 420, 421); // Convert x/y into Vector2
		Main.PlaySound(SoundID.MenuTick, (int)player.position.X, (int)player.position.Y); // Simplify manual x/y int conversion into Vector2
		Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 17); // Also simplify style if combinable with type

		Main.PlaySound(new LegacySoundStyle(2, 10), player.position); // The following four have dedicated numbered SoundStyles
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