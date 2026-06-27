using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomVisualEquipType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomVisualEquipType/README.md

	/// <summary>
	/// EarringsDrawLayer is a PlayerDrawLayer that renders a custom EquipType. 
	/// </summary>
	public class EarringsDrawLayer : PlayerDrawLayer
	{
		public override bool IsHeadLayer => true;

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<EarringsPlayer>().earring > 0;
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			EarringsPlayer earringsPlayer = drawPlayer.GetModPlayer<EarringsPlayer>();
			int earring = earringsPlayer.earring;
			var earringTexture = EarringsLoader.earringItemToTexture[earring];

			// Calculate where to draw this layer. 
			var drawPosition = new Vector2(
				(int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2),
				(int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)
			) + drawPlayer.headPosition + drawInfo.headVect + Main.OffsetsPlayerHeadgear[drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height] * drawPlayer.gravDir;

			// Source rectangle. Player.bodyFrame corresponds to the animation frame (pose) that the player is currently in.
			// Vanilla player layers for EquipTypes have a separate image for each pose to account for how layers move in relation to each other while the player is animating, but this example doesn't need that so we use the same image all the time.
			Rectangle drawFrame = drawPlayer.bodyFrame;
			drawFrame.Y = 0;

			var drawData = new DrawData(
				earringTexture.Value,
				drawPosition,
				drawFrame,
				drawInfo.colorArmorHead,
				drawPlayer.headRotation,
				drawInfo.headVect, // Origin
				1f,
				drawInfo.playerEffect
			);
			drawData.shader = earringsPlayer.earringShader;
			drawInfo.DrawDataCache.Add(drawData);
		}
	}
}
