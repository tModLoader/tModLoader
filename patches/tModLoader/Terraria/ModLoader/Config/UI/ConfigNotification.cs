using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

public class ConfigNotification : IInGameNotification
{
	public bool ShouldBeRemoved => timeLeft <= 0;
	private Asset<Texture2D> Icon => UICommon.ButtonModConfigTexture;

	private int timeLeft;
	private string message;
	private Color color;

	public ConfigNotification(string message, Color color, int timeLeft = 5 * 60)
	{
		this.message = message;
		this.color = color; // TODO: use the color
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

		string title = message;
		var textColor = Main.MouseTextColorReal * Opacity;
		textColor = textColor.MultiplyRGBA(color);

		float effectiveScale = Scale * 1.1f;
		Vector2 size = (FontAssets.ItemStack.Value.MeasureString(title) + new Vector2(64f, 10f)) * effectiveScale;
		Rectangle panelSize = new Rectangle((int)(bottomAnchorPosition.X - size.X), (int)(bottomAnchorPosition.Y - size.Y), (int)size.X, (int)size.Y);

		bool hovering = panelSize.Contains(Main.MouseScreen.ToPoint());

		Utils.DrawInvBG(spriteBatch, panelSize, new Color(64, 109, 164) * (hovering ? 0.75f : 0.5f));
		float iconScale = effectiveScale * 0.7f;
		Vector2 vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (0f + iconScale * Icon.Width());
		spriteBatch.Draw(Icon.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, Icon.Width() / 2f), iconScale, SpriteEffects.None, 0f);
		Utils.DrawBorderString(color: textColor, sb: spriteBatch, text: title, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

		if (hovering) {
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
