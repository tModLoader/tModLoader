using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

public class ModBossBarTest : ModBossBar
{
	public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float lifePercent, ref float shieldPercent) => null;

	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
		float lifePercent = drawParams.LifePercentToShow;
		float shieldPercent = drawParams.ShieldPercentToShow;
		return false;
	}
}