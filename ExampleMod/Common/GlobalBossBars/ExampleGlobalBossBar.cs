using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalBossBars
{
	// Shows things you can do around drawing boss bars
	public class ExampleGlobalBossBar : GlobalBossBar
	{
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			if (npc.type == NPCID.EyeofCthulhu) {
				drawParams.IconColor = Main.DiscoColor;
			}

			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) {
			if (npc.type == NPCID.EyeofCthulhu) {
				string text = "GlobalBossBar Showcase";
				var font = FontAssets.MouseText.Value;
				Vector2 size = font.MeasureString(text);
				// Draw centered on the boss bar, offset upwards, otherwise it will overlap with the health text
				spriteBatch.DrawString(font, text, drawParams.BarCenter - size / 2 + new Vector2(0, -30), Color.White);
			}
		}
	}
}
