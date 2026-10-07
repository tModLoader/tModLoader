using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;
public partial class WorldItem
{
	public bool master => inner.master;

	public ModItem ModItem => inner.ModItem;

	public void ProcessItemLiquidSplashModded(bool isEnter)
	{
		bool inModdedLiquid = false;
		for (int k = LiquidID.Count; k < LiquidLoader.LiquidCount; k++) {
			if (wets[k]) {
				if (LiquidLoader.OnItemSplash(k, this, true)) {
					ModLiquid modLiquid = LiquidLoader.GetLiquid(k);
					if (modLiquid.OnItemSplash(this, isEnter)) {
						for (int i = 0; i < 10; i++) {
							if(modLiquid.SplashDustType >= 0) {
								int newDust = Dust.NewDust(new Vector2(position.X - 6f, position.Y + (float)(height / 2) - 8f), width + 12, 24, modLiquid.SplashDustType);
								Main.dust[newDust].velocity.Y -= 4f;
								Main.dust[newDust].velocity.X *= 2.5f;
								Main.dust[newDust].scale *= 0.8f;
								Main.dust[newDust].alpha = 100;
								Main.dust[newDust].noGravity = true;
							}
						}
						SoundEngine.PlaySound(modLiquid.SplashSound, position);
					}
				}

				inModdedLiquid = true;
			}
		}

		if (!inModdedLiquid && LiquidLoader.OnItemSplash(LiquidID.Water, this, isEnter)) {
			for (int i = 0; i < 10; i++) {
				int newDust = Dust.NewDust(new Vector2(position.X - 6f, position.Y + (float)(height / 2) - 8f), width + 12, 24, Dust.dustWater());
				Main.dust[newDust].velocity.Y -= 4f;
				Main.dust[newDust].velocity.X *= 2.5f;
				Main.dust[newDust].scale *= 0.8f;
				Main.dust[newDust].alpha = 100;
				Main.dust[newDust].noGravity = true;
			}
		}
	}
}
