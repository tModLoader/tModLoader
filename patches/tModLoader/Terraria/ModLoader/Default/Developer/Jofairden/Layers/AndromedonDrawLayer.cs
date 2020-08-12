using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal abstract class AndromedonDrawLayer : PlayerDrawLayer
	{
		protected static int? ShaderId;

		public abstract DrawDataInfo GetData(PlayerDrawSet info);

		public override void GetDefaults(Player drawPlayer, out bool visible, out float depth) {
			visible = drawPlayer.GetModPlayer<DeveloperPlayer>().AndromedonEffect.LayerStrength > 0f;
			depth = 0f;
		}

		public static DrawDataInfo GetHeadDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = drawPlayer.headPosition + drawInfo.headVect + drawInfo.Position - Main.screenPosition + new Vector2(
				drawPlayer.width / 2f - drawPlayer.bodyFrame.Width / 2f,
				drawPlayer.height - drawPlayer.bodyFrame.Height + 4f
			);

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.headVect,
				Rotation = drawPlayer.headRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetBodyDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = drawPlayer.bodyPosition + drawInfo.bodyVect + drawInfo.Position - Main.screenPosition + new Vector2(
				-drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f,
				drawPlayer.height - drawPlayer.bodyFrame.Height + 4f
			);

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.bodyVect,
				Rotation = drawPlayer.bodyRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetLegDrawDataInfo(PlayerDrawSet drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = drawInfo.legVect + drawPlayer.legPosition + drawInfo.Position - Main.screenPosition + new Vector2(
				-drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f,
				drawPlayer.height - drawPlayer.legFrame.Height + 4f
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
}
