using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using ReLogic.Utilities;

public class SoundTest : ModProjectile
{
	void Method(Player player) {
		// This is the most straightforward overload to port: SoundID.Something which usually has an equivalent in 1.4s SoundID. SoundEngine requires Terraria.Audio, SoundID.MenuTick is SoundStyle and not int like 1.
		Main.PlaySound(SoundID.MenuTick, player.position);

		// default parameter variations
		Main.PlaySound(SoundID.MenuTick);
		Main.PlaySound(SoundID.MenuTick, -1);
		Main.PlaySound(SoundID.MenuTick, -1, -1);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 1);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 1, 1f);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 1, 1, 0);

		Main.PlaySound(SoundID.MenuTick, 420, 421); // Convert x/y into Vector2
		Main.PlaySound(SoundID.MenuTick, (int)player.position.X, (int)player.position.Y); // Simplify manual x/y int conversion into Vector2
		Main.PlaySound(SoundID.MenuTick, -1, -1, 1, 1.1f);
		Main.PlaySound(SoundID.MenuTick, -1, -1, 0, 1, 0.2f);
		Main.PlaySound(SoundID.MenuTick, 1, 2, 0, 1.1f, 0.2f);

		// named pitch variance or volume
		Main.PlaySound(SoundID.MenuTick, volumeScale: 2f);
		Main.PlaySound(SoundID.MenuTick, pitchOffset: 0.1f);
		Main.PlaySound(SoundID.MenuTick, volumeScale: 2f, pitchOffset: 0.1f);

		// sounds with styles
		Main.PlaySound(SoundID.Splash); // change to SplashWeak
		Main.PlaySound(SoundID.Splash, -1, -1, 0); // splash needs style 0
		Main.PlaySound(SoundID.Splash, -1, -1, 1); // this is SplashWeak
		Main.PlaySound(SoundID.Mech, Style: 0); // mech previously needed style 0
		Main.PlaySound(SoundID.Roar); // change to WormDig
		Main.PlaySound(SoundID.Roar, Style: 0);
		Main.PlaySound(SoundID.Roar, Style: 1);
		Main.PlaySound(SoundID.Roar, Style: 2);
		Main.PlaySound(SoundID.Roar, Style: 4);
		Main.PlaySound(new LegacySoundStyle(SoundID.Roar, 4), player.position);
		Main.PlaySound(SoundID.ForceRoar);
		Main.PlaySound(SoundID.ForceRoar, Style: -1);

		// some of these can't be refactored properly (for the SoundID int -> style change) because the default style sound has changed name
		SoundEngine.PlaySound(SoundID.Splash); // can't tell if the modder is aware of the new SoundStyle, or was previously using the int const
		SoundEngine.PlaySound(19);
		SoundEngine.PlaySound(SoundID.Splash, Style: 0);
		SoundEngine.PlaySound(SoundID.Splash, Style: 1);
		SoundEngine.PlaySound(SoundID.Roar); // can't tell if the modder is aware of the new SoundStyle, or was previously using the int const
		SoundEngine.PlaySound(15);
		SoundEngine.PlaySound(SoundID.Roar, Style: 0);
		SoundEngine.PlaySound(SoundID.Roar, Style: 1);

		// sounds with styles that map to `Style{n}`
		Main.PlaySound(SoundID.Item, player.position, 172);
		Main.PlaySound(SoundID.NPCHit, player.position, 57);
		Main.PlaySound(SoundID.NPCKilled, player.position, 65);
		Main.PlaySound(SoundID.Zombie, player.position, 117);

		Main.PlaySound(new LegacySoundStyle(2, 1), player.position);
		Main.PlaySound(new LegacySoundStyle(3, 2), player.position);
		Main.PlaySound(new LegacySoundStyle(4, 3), player.position);
		Main.PlaySound(new LegacySoundStyle(29, 4), player.position);

		Main.PlaySound(2, player.position, 10);
		Main.PlaySound(3, player.position, 11);
		Main.PlaySound(4, player.position, 12);
		Main.PlaySound(29, player.position, 13);

		Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 17);

		// other sound methods
		var slotId = Main.PlayTrackedSound(SoundID.BlizzardStrongLoop);
		var sound = Main.GetActiveSound(slotId);
		
		projectile.localAI[0] = Main.PlayTrackedSound(SoundID.DD2_PhantomPhoenixShot, projectile.Center).ToFloat();
		sound = Main.GetActiveSound(SlotId.FromFloat(projectile.localAI[0]));

		Main.GetActiveSound(slotId);
	}
}