using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.ModLoader.UI.ModBrowser;

public enum ModDownloadStatusState
{
	Queued,
	Downloading,
}

// TODO: UIAnimatedImage has hover changes, while our other buttons for UIModDownloadItem do not.
public class UIModDownloadStatus : UIAnimatedImage
{
	private static Asset<Texture2D> Texture => UICommon.ModDownloadIndicatorTexture;

	public UIModDownloadStatus() : base(Texture, 36, 36, 0, 0, 1, 5, 2)
	{
		ColorNotHovered = Color.White;
		SetCurrentState(ModDownloadStatusState.Queued);
	}

	public void SetCurrentState(ModDownloadStatusState state)
	{
		switch (state) {
			case ModDownloadStatusState.Downloading:
				FrameStart = 0;
				FrameCount = 4;
				break;
			case ModDownloadStatusState.Queued:
				FrameStart = 4;
				FrameCount = 1;
				break;
		}
	}
}
