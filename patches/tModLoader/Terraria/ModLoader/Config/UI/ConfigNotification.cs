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

	private int timeLeft;
	private Asset<Texture2D> icon;
	private string message;
	private Color textColor;
	private Color panelColor;
	private Color panelColorHover;

	public ConfigNotification(Asset<Texture2D> icon, string message, Color textColor, Color panelColor = default, Color panelColorHover = default, int timeLeft = 5 * 60)
	{
		if (panelColor == default)
			panelColor = new Color(64, 109, 164) * 0.5f;

		if (panelColorHover == default)
			panelColorHover = new Color(64, 109, 164) * 0.75f;

		this.icon = icon;
		this.message = message;
		this.textColor = textColor;
		this.panelColor = panelColor;
		this.panelColorHover = panelColorHover;
		this.timeLeft = timeLeft;
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

		var iconSize = icon.Size();
		var targetIconSize = new Vector2(64, 64);
		float effectiveScale = Scale * 1.1f;
		Vector2 size = (FontAssets.ItemStack.Value.MeasureString(message) + new Vector2(58f, 10f)) * effectiveScale;
		Rectangle panelSize = Utils.CornerRectangle(bottomAnchorPosition - size, bottomAnchorPosition);
		bool isHovering = panelSize.Contains(Main.MouseScreen.ToPoint());
		float iconScaleAdjustment = targetIconSize.X / icon.Width(); // Makes any icon take up the same area the achievement icons take up
		float iconScale = effectiveScale * 0.3f;
		Vector2 vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (12f + iconScale * targetIconSize.X);

		Utils.DrawInvBG(c: isHovering ? panelColorHover : panelColor, sb: spriteBatch, R: panelSize);
		spriteBatch.Draw(icon.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, iconSize.Y / 2), iconScale * iconScaleAdjustment, SpriteEffects.None, 0f);
		Utils.DrawBorderString(color: textColor.MultiplyRGBA(Main.MouseTextColorReal) * Opacity, sb: spriteBatch, text: message, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

		if (isHovering) {
			OnMouseOver();
		}
	}

	private void OnMouseOver() {
		if (PlayerInput.IgnoreMouseInterface) {
			return;
		}

		Main.LocalPlayer.mouseInterface = true;

		if (timeLeft <= 60 * 2 && timeLeft > 30) {
			timeLeft = 60 * 2;
		}

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
