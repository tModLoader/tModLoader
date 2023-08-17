using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

public class ModBossBarTest : ModBossBar
{
	public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)/* tModPorter Note: life and shield current and max values are now separate to allow for hp/shield number text draw */ => null;

	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
#if COMPILE_ERROR
		// not-yet-implemented
		float lifePercent = drawParams.LifePercentToShow;
		float shieldPercent = drawParams.ShieldPercentToShow;
		// instead-expect
		float lifePercent = drawParams.LifePercentToShow/* tModPorter Note: Removed. Suggest: Life / LifeMax */;
		float shieldPercent = drawParams.ShieldPercentToShow/* tModPorter Note: Removed. Suggest: Shield / ShieldMax */;
#endif
		return false;
	}
}