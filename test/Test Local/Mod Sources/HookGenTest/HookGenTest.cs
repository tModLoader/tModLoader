using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using OnMain = On.Terraria.Main;

namespace HookGenTest
{
	class HookGenTest : Mod
	{
		const int mode = 0;
		
		public override void Load() {
			if (mode > 0)
				MonoModHooks.RequestNativeAccess();
			
			switch (mode) {
			case 0:
				OnMain.DrawCursor += Wrap;
				break;
			case 1:
				new Hook(new OnMain.orig_DrawCursor(Main.DrawCursor), new OnMain.hook_DrawCursor(Wrap));
				break;
			case 2:
				new Detour(new OnMain.orig_DrawCursor(Main.DrawCursor), new OnMain.orig_DrawCursor(Replace));
				break;
			case 3:
				new NativeDetour(new OnMain.orig_DrawCursor(Main.DrawCursor), new OnMain.orig_DrawCursor(Replace));
				break;
			}
		}
		
		private static void Wrap(On.Terraria.Main.orig_DrawCursor orig, Vector2 bonus, bool smart) {
			orig(bonus, smart);
			orig(new Vector2(30, 0), smart);
		}
		
		private static void Replace(Vector2 bonus, bool smart) {
			Main.spriteBatch.Draw(Main.cursorTextures[0], new Vector2((float)Main.mouseX, (float)Main.mouseY), null, Color.Green, 0f, default(Vector2), Main.cursorScale, SpriteEffects.None, 0f);
		}
	}
}
