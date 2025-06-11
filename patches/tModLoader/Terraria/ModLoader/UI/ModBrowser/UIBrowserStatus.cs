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
		SetCurrentState(AsyncProviderState.Completed);
	}

	public void SetCurrentState(AsyncProviderState state)
	{
		switch (state) {
			case AsyncProviderState.Loading:
				FrameStart = 0;
				FrameCount = 4;
				break;
			case AsyncProviderState.Canceled:
			case AsyncProviderState.Aborted:
				FrameStart = 4;
				FrameCount = 1;
				break;
			case AsyncProviderState.Completed:
				FrameStart = 5;
				FrameCount = 1;
				break;
		}
	}
}
