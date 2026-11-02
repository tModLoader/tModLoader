using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

public class ConfigNotification : IInGameNotification
{
	public bool ShouldBeRemoved => timeLeft <= 0;
	private Asset<Texture2D> Icon => UICommon.ButtonModConfigNotificationTexture;

	private int timeLeft;
	private string message;
	private Color color;

	public ConfigNotification(string message, Color color, int timeLeft = 5 * 60)
	{
		this.message = message;
		this.color = color;
		this.timeLeft = timeLeft;

		// We don't want to change the colour that mods use, since they can always change the colour themselves this way if they want to
		if (this.color == Color.Green)
			this.color = new Color(255, 255, 255 / 5);
	}

	private float Scale {
		get {
			if (timeLeft < 30) {
				return MathHelper.Lerp(0f, 1f, timeLeft / 30f);
			}

			if (timeLeft > 285) {
				return MathHelper.Lerp(1f, 0f, (timeLeft - 285) / 15f);
			}

			return 1f;
		}
	}

	private float Opacity {
		get {
			if (Scale <= 0.5f) {
				return 0f;
			}

			return (Scale - 0.5f) / 0.5f;
		}
	}

	public void Update() {
		timeLeft--;

		if (timeLeft < 0) {
			timeLeft = 0;
		}
	}

	public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition) {
		if (Opacity <= 0f) {
			return;
		}

		var iconSize = Icon.Size();
		var targetIconSize = new Vector2(64, 64);
		float effectiveScale = Scale * 1.1f;
		Vector2 size = (FontAssets.ItemStack.Value.MeasureString(message) + new Vector2(58f, 10f)) * effectiveScale;
		Rectangle panelSize = Utils.CornerRectangle(bottomAnchorPosition - size, bottomAnchorPosition);
		bool isHovering = panelSize.Contains(Main.MouseScreen.ToPoint());
		float iconScaleAdjustment = 64f / Icon.Width(); // Makes any icon take up the same area the achievement icons take up
		float iconScale = effectiveScale * 0.3f;
		Vector2 vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (12f + iconScale * targetIconSize.X);

		Utils.DrawInvBG(c: new Color(64, 109, 164) * (isHovering ? 0.75f : 0.5f), sb: spriteBatch, R: panelSize);
		spriteBatch.Draw(Icon.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, iconSize.Y / 2), iconScale * iconScaleAdjustment, SpriteEffects.None, 0f);
		Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, (int)Main.mouseTextColor / 5, Main.mouseTextColor) * Opacity, sb: spriteBatch, text: message, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

		if (isHovering) {
			OnMouseOver();
		}
	}

	private void OnMouseOver() {
		if (PlayerInput.IgnoreMouseInterface) {
			return;
		}

		Main.LocalPlayer.mouseInterface = true;

		if (!Main.mouseLeft || !Main.mouseLeftRelease) {
			return;
		}

		Main.mouseLeftRelease = false;

		if (timeLeft > 30) {
			timeLeft = 30;
		}
	}

	public void PushAnchor(ref Vector2 positionAnchorBottom) {
		positionAnchorBottom.Y -= 50f * Opacity;
	}
}
