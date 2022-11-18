using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

internal abstract class JofairdenArmorDrawLayer : PlayerDrawLayer
{
	protected static int? ShaderId;

	public abstract DrawDataInfo GetData(PlayerDrawSet info);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		return drawInfo.shadow == 0f && !player.invis && player.GetModPlayer<JofairdenArmorEffectPlayer>().LayerStrength > 0f;
	}

	public static DrawDataInfo GetHeadDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture)
	{
		Player drawPlayer = drawInfo.drawPlayer;
		Vector2 pos = drawPlayer.headPosition + drawInfo.headVect + new Vector2(
			(int)(drawInfo.Position.X + drawPlayer.width / 2f - drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X),
			(int)(drawInfo.Position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y)
		);

		return new DrawDataInfo {
			Position = pos,
			Frame = drawPlayer.bodyFrame,
			Origin = drawInfo.headVect,
			Rotation = drawPlayer.headRotation,
			Texture = texture
		};
	}

	public static DrawDataInfo GetBodyDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture)
	{
		Player drawPlayer = drawInfo.drawPlayer;
		Vector2 pos = drawPlayer.bodyPosition + drawInfo.bodyVect + new Vector2(
			 (int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f),
			 (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)
		);

		return new DrawDataInfo {
			Position = pos,
			Frame = drawPlayer.bodyFrame,
			Origin = drawInfo.bodyVect,
			Rotation = drawPlayer.bodyRotation,
			Texture = texture
		};
	}

	public static DrawDataInfo GetLegDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture)
	{
		Player drawPlayer = drawInfo.drawPlayer;
		Vector2 pos = drawPlayer.legPosition + drawInfo.legVect + new Vector2(
			(int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f),
			(int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)
		);

		return new DrawDataInfo {
			Position = pos,
			Frame = drawPlayer.legFrame,
			Origin = drawInfo.legVect,
			Rotation = drawPlayer.legRotation,
			Texture = texture
		};
	}
}
