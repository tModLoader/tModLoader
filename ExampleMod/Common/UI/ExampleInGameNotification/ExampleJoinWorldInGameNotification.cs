using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleInGameNotification
{
	// This is a custom implementation of IInGameNotification for usage with the InGameNotificationSystem class.
	// It displays a welcome message to the player when they join a world, controlled through ExampleInGameNotificationPlayer.
	public class ExampleJoinWorldInGameNotification : IInGameNotification
	{
		// Remove this notification once the 5-second timer is up.
		public bool ShouldBeRemoved => timeLeft <= 0;

		// 5 seconds, controls how long this notification lasts for.
		private int timeLeft = 5 * 60;

		// The texture we'll use for our icon display.
		// Let's keep it simple and use use the ExampleItem's sprite.
		private Asset<Texture2D> iconTexture = TextureAssets.Item[ModContent.ItemType<ExampleItem>()];

		// The Scale and Opacity properties are used to control the scale and opacity of the UI popup,
		// and are directly taken from the vanilla achievement popup UI. This is done for consistency.
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

		// See the comments for Scale.
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

			// Keep the timer kept to a minimum value of 0 to avoid issues, since we
			// use it for lerping and other effects.
			if (timeLeft < 0) {
				timeLeft = 0;
			}
		}

		public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition) {
			// No reason to continue drawing if the notification is no longer visible.

			if (Opacity <= 0f) {
				return;
			}

			string title = Language.GetTextValue("Mods.ExampleMod.UI.InGameNotificationTitle");

			// Below is draw-code directly from vanilla with some tweaks to suit our needs.
			// Changes are minimal; important things to note:
			// - we draw the panel with Utils.DrawInvBG,
			// - we calculate the panel size based on the title size,
			// - we draw the title and icon after the panel,
			// - we utilize the calculated opacity and scale values.

			float effectiveScale = Scale * 1.1f;
			Vector2 size = (FontAssets.ItemStack.Value.MeasureString(title) + new Vector2(58f, 10f)) * effectiveScale;
			Rectangle panelSize = Utils.CenteredRectangle(bottomAnchorPosition + new Vector2(0f, (0f - size.Y) * 0.5f), size);

			// Check if the mouse is hovering over the notification.
			bool hovering = panelSize.Contains(Main.MouseScreen.ToPoint());

			Utils.DrawInvBG(spriteBatch, panelSize, new Color(64, 109, 164) * (hovering ? 0.75f : 0.5f));
			float iconScale = effectiveScale * 0.7f;
			Vector2 vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (12f + iconScale * iconTexture.Width());
			spriteBatch.Draw(iconTexture.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, iconTexture.Width() / 2f), iconScale, SpriteEffects.None, 0f);
			Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor / 5, Main.mouseTextColor) * Opacity, sb: spriteBatch, text: title, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

			if (hovering) {
				OnMouseOver();
			}
		}

		private void OnMouseOver() {
			// This method is called when the user hovers over the notification.

			// Skip if we're ignoring mouse input.
			if (PlayerInput.IgnoreMouseInterface) {
				return;
			}

			// We are now interacting with a UI.
			Main.LocalPlayer.mouseInterface = true;

			if (!Main.mouseLeft || !Main.mouseLeftRelease) {
				return;
			}

			Main.mouseLeftRelease = false;

			// In our example, we just accelerate the exiting process on click.
			// If you want it to close immediately, you can just set timeLeft to 0.
			// This allows the notification time to shrink and fade away, as expected.
			if (timeLeft > 30) {
				timeLeft = 30;
			}
		}

		public void PushAnchor(ref Vector2 positionAnchorBottom) {
			// Anchoring is used for determining how much space a popup takes up, essentially.
			// This is because notifications visually stack. In our case, we want to let other notifications
			// go in front of ours once we start fading off, so we scale the offset based on opacity.
			positionAnchorBottom.Y -= 50f * Opacity;
		}
	}
}