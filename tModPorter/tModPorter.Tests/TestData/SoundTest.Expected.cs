using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework;

//TODO modded sound retrieval&playback
//TODO non-dedicated IDs (Compile_Error req?)
//TODO pitch + volume
public class SoundTest
{
	void Method() {
		Player player = new Player();

		SoundEngine.PlaySound(SoundID.MenuTick, player.position); // This is the most straightforward overload to port: SoundID.Something which usually has an equivalent in 1.4s SoundID. SoundEngine requires Terraria.Audio, SoundID.MenuTick is SoundStyle and not int like 1.3

		SoundEngine.PlaySound(SoundID.MenuTick); // The following are different, using the most generic overload with all parameters
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);

		SoundEngine.PlaySound(SoundID.MenuTick, new Vector2(420, 421)); // Convert x/y into Vector2
		SoundEngine.PlaySound(SoundID.MenuTick, player.position); // Simplify manual x/y int conversion into Vector2
		SoundEngine.PlaySound(SoundID.Item17, player.Center); // Also simplify style if combinable with type

		SoundEngine.PlaySound(SoundID.Item10, player.position); // The following four have dedicated numbered SoundStyles
		SoundEngine.PlaySound(SoundID.NPCHit10, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath10, player.position);
		SoundEngine.PlaySound(SoundID.Zombie10, player.position);

		SoundEngine.PlaySound(SoundID.Item10, player.position);
		SoundEngine.PlaySound(SoundID.NPCHit10, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath10, player.position);
		SoundEngine.PlaySound(SoundID.Zombie10, player.position);

		SoundEngine.PlaySound(SoundID.Item10, player.position);
		SoundEngine.PlaySound(SoundID.NPCHit10, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath10, player.position);
		SoundEngine.PlaySound(SoundID.Zombie10, player.position);
	}
}