using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

//TODO: Glowmasks should be simplified for everyone.
internal abstract class JofairdenArmorGlowLayer : JofairdenArmorDrawLayer
{
	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		var drawDataInfo = GetData(drawInfo);
		var drawPlayer = drawInfo.drawPlayer;
		var modPlayer = drawPlayer.GetModPlayer<JofairdenArmorEffectPlayer>();
		var effects = SpriteEffects.None;

		if (drawPlayer.direction == -1) {
			effects |= SpriteEffects.FlipHorizontally;
		}

		if (drawPlayer.gravDir == -1) {
			effects |= SpriteEffects.FlipVertically;
		}

		var data = new DrawData(
			drawDataInfo.Texture,
			drawDataInfo.Position,
			drawDataInfo.Frame,
			Color.White * Main.essScale * modPlayer.LayerStrength,
			drawDataInfo.Rotation,
			drawDataInfo.Origin,
			1f,
			effects,
			0
		);

		if (modPlayer.HasAura) {
			ShaderId ??= GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye);
			data.shader = ShaderId.Value;
		}

		drawInfo.DrawDataCache.Add(data);
	}
}
