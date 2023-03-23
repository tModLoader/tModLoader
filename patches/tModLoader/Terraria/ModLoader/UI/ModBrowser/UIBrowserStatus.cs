using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser;

public class UIBrowserStatus : UIAnimatedImage
{
	private static Asset<Texture2D> Texture => UICommon.ModBrowserIconsTexture;

	public UIBrowserStatus()
		: base(Texture, 32, 32, 34 * 6, 0, 1, 6, 2)
	{
		SetCurrentState(AsyncProvider.State.NotStarted);
	}

	public void SetCurrentState(AsyncProvider.State state)
	{
		switch (state) {
			case AsyncProvider.State.NotStarted:
			case AsyncProvider.State.Loading:
				FrameStart = 0;
				FrameCount = 4;
				break;
			case AsyncProvider.State.Aborted:
				FrameStart = 4;
				FrameCount = 1;
				break;
			case AsyncProvider.State.Completed:
				FrameStart = 5;
				FrameCount = 1;
				break;
		}
	}
}
