using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser;

public class UIBrowserFilterToggle<T> : UICycleImage where T : struct, Enum
{
	private static Asset<Texture2D> Texture => UICommon.ModBrowserIconsTexture;

	public T State {
		get;
		private set;
	}

	//TODO: Needs to update texture and logic
	public UIBrowserFilterToggle(int textureOffsetX, int textureOffsetY, int padding = 2)
		: base(Texture, Enum.GetValues(typeof(T)).Length, 32, 32, textureOffsetX, textureOffsetY, padding)
	{
		OnLeftClick += UpdateToNext;
		OnRightClick += UpdateToPrevious;
	}

	public void SetCurrentState(T @enum)
	{
		State = @enum;
		base.SetCurrentState((int)(object)State);
	}

	private void UpdateToNext(UIMouseEvent @event, UIElement element)
	{
		SetCurrentState(State.NextEnum());
	}

	private void UpdateToPrevious(UIMouseEvent @event, UIElement element)
	{
		SetCurrentState(State.PreviousEnum());
	}
}