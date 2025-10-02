using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser;

public class ModDownloadNotification : IInGameNotification, IDownloadProgress
{
	// Remove this notification once the download completes and then the 3-second timer is up.
	public bool ShouldBeRemoved => timeLeft <= 0;

	private int timeLeft = 3 * 60;

	private int animationTimer = 0;

	private string message;
	private ModDownloadItem downloadItem;

	public ModDownloadNotification(ModDownloadItem downloadItem)
	{
		this.downloadItem = downloadItem;
		message = $"Download pending {downloadItem.DisplayName}";
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

	public void Update()
	{
		animationTimer++;
		if (!downloadItem.IsInstalled) // TODO: Sometimes this stays false somehow -> Fixed by calling ModOrganizer.LocalModsChanged
			return;

		timeLeft--;

		if (timeLeft < 0) {
			timeLeft = 0;
		}
	}

	public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
	{
		if (Opacity <= 0f) {
			return;
		}

		string title = message;

		Asset<Texture2D> icon = UICommon.ModDownloadIndicatorTexture;
		Rectangle frame = icon.Frame(1, 6, 0, 4, 0, -2);

		if(downloadItem.Downloading) {
			frame = icon.Frame(1, 6, 0, animationTimer / 4 % 4, 0, -2);
		}
		if (downloadItem.IsInstalled) {
			frame = icon.Frame(1, 6, 0, 5, 0, -2);
			message = $"Downloading {downloadItem.DisplayName}: Complete!";
			title = message;
		}

		float effectiveScale = Scale * 1.1f;
		Vector2 size = (FontAssets.ItemStack.Value.MeasureString(title) + new Vector2(64f, 10f)) * effectiveScale;
		Rectangle panelSize = new Rectangle((int)(bottomAnchorPosition.X - size.X), (int)(bottomAnchorPosition.Y - size.Y), (int)size.X, (int)size.Y);

		bool hovering = panelSize.Contains(Main.MouseScreen.ToPoint());

		Utils.DrawInvBG(spriteBatch, panelSize, new Color(64, 109, 164) * (hovering ? 0.75f : 0.5f));
		float iconScale = effectiveScale * 0.7f;
		Vector2 vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (6f + iconScale * icon.Width());
		spriteBatch.Draw(icon.Value, vector, frame, Color.White * Opacity, 0f, new Vector2(0f, icon.Width() / 2f), iconScale, SpriteEffects.None, 0f);
		Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor / 5, Main.mouseTextColor) * Opacity, sb: spriteBatch, text: title, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

		if (hovering) {
			OnMouseOver();
		}
	}

	private void OnMouseOver()
	{
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

	public void PushAnchor(ref Vector2 positionAnchorBottom)
	{
		positionAnchorBottom.Y -= 50f * Opacity;
	}

	public void DownloadStarted(string displayName)
	{
	}

	public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded)
	{
		message = $"Downloading {downloadItem.DisplayName}: {(float)bytesReceived/totalBytesNeeded:P0}";

		if(bytesReceived == totalBytesNeeded) {
			message = $"Downloading {downloadItem.DisplayName}: Complete!";
		}
	}
}
