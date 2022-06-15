using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using ReLogic.Utilities;

public class SoundTest : ModProjectile
{
	void Method(Player player) {
		// This is the most straightforward overload to port: SoundID.Something which usually has an equivalent in 1.4s SoundID. SoundEngine requires Terraria.Audio, SoundID.MenuTick is SoundStyle and not int like 1.
		SoundEngine.PlaySound(SoundID.MenuTick, player.position);

		// default parameter variations
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);
		SoundEngine.PlaySound(SoundID.MenuTick);

		// named pitch variance or volume
		SoundEngine.PlaySound(SoundID.MenuTick.WithVolumeScale(2f));
		SoundEngine.PlaySound(SoundID.MenuTick.WithPitchOffset(0.1f));
		SoundEngine.PlaySound(SoundID.MenuTick.WithVolumeScale(2f).WithPitchOffset(0.1f));
		SoundEngine.PlaySound(SoundID.MenuTick.WithPitchOffset(0.1f), player.position);

		SoundEngine.PlaySound(SoundID.MenuTick, new Vector2(420, 421)); // Convert x/y into Vector2
		SoundEngine.PlaySound(SoundID.MenuTick, player.position); // Simplify manual x/y int conversion into Vector2

		// sounds with styles
		SoundEngine.PlaySound(SoundID.Splash);
		SoundEngine.PlaySound(SoundID.SplashWeak);
		SoundEngine.PlaySound(SoundID.SplashWeak);
		SoundEngine.PlaySound(SoundID.Mech);
		SoundEngine.PlaySound(SoundID.WormDig);
		SoundEngine.PlaySound(SoundID.Roar);
		SoundEngine.PlaySound(SoundID.WormDig);
		SoundEngine.PlaySound(SoundID.ScaryScream);
		SoundEngine.PlaySound(SoundID.WormDigQuiet);
		SoundEngine.PlaySound(SoundID.WormDigQuiet, player.position);

		// sounds with styles that map to `Style{n}`
		SoundEngine.PlaySound(SoundID.Item172, player.position); // The following four have dedicated numbered SoundStyles
		SoundEngine.PlaySound(SoundID.NPCHit57, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath65, player.position);
		SoundEngine.PlaySound(SoundID.Zombie117, player.position);

		SoundEngine.PlaySound(SoundID.Item1, player.position);
		SoundEngine.PlaySound(SoundID.NPCHit2, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath3, player.position);
		SoundEngine.PlaySound(SoundID.Zombie4, player.position);

		SoundEngine.PlaySound(SoundID.Item10, player.position);
		SoundEngine.PlaySound(SoundID.NPCHit11, player.position);
		SoundEngine.PlaySound(SoundID.NPCDeath12, player.position);
		SoundEngine.PlaySound(SoundID.Zombie13, player.position);

		SoundEngine.PlaySound(SoundID.Item17, new Vector2((int)player.Center.X, (int)player.Center.Y));

		// other sound methods
		var slotId = SoundEngine.PlaySound(SoundID.BlizzardStrongLoop);
		SoundEngine.TryGetActiveSound(slotId, out var sound);
		
		Projectile.localAI[0] = SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot, Projectile.Center).ToFloat();
		SoundEngine.TryGetActiveSound(SlotId.FromFloat(Projectile.localAI[0]), out sound);
	}
}