using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Graphics;
using Terraria.DataStructures;

namespace ExampleMod.Common.GlobalBossBars
{
	//Shows things you can do around drawing boss bars
	public class ExampleGlobalBossBar : GlobalBossBar
	{
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			if (npc.type == NPCID.EyeofCthulhu) {
				drawParams.iconColor = Main.DiscoColor;
			}

			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) {
			if (npc.type == NPCID.EyeofCthulhu) {
				string text = "GlobalBossBar Showcase";
				var font = FontAssets.MouseText.Value;
				Vector2 size = font.MeasureString(text);
				spriteBatch.DrawString(font, text, drawParams.barCenter - size / 2, Color.White);
			}
		}
	}
}
