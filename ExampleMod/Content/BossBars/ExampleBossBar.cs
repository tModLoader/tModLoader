using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Content.BossBars
{
	//Shows basic boss bar code using a custom colored texture. It only does visual things, so for a more practical boss bar, see the other example (MinionBossBossBar)
	//To use this, in an NPCs SetDefaults, write:
	//  NPC.BossBar = ModContent.GetInstance<ExampleBossBar>();

	//Keep in mind that if the NPC has a boss head icon, it will automatically have the common boss health bar from vanilla. A ModBossBar is not mandatory for a boss.

	//You can make it so your NPC never shows a boss bar, such as Dungeon Guardian or Lunatic Cultist Clone:
	//  NPC.BossBar = Main.BigBossProgressBar.NeverValid;
	public class ExampleBossBar : ModBossBar
	{
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return TextureAssets.NpcHead[36]; //Corgi head icon
		}

		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			//Make the bar shake the less health the NPC has
			float shakeIntensity = Utils.Clamp(1f - drawParams.lifePercentToShow - 0.2f, 0f, 1f);
			drawParams.barCenter.Y -= 20f;
			drawParams.barCenter += new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * shakeIntensity * 15f;

			drawParams.iconColor = Main.DiscoColor;

			return true;
		}
	}
}
