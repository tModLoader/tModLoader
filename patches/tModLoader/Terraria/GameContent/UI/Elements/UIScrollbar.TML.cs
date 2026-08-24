using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.GameContent.UI.Elements;

public partial class UIScrollbar : UIElement
{
	public float ViewSize => _viewSize;
	public float MaxViewSize => _maxViewSize;

	// Used by UIWorldCreationAdvanced
	public event Action OnScroll;
	private float _prevViewPosition;

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (IsMouseHovering)
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIList");
		if(_prevViewPosition != _viewPosition)
			OnScroll?.Invoke();
		_prevViewPosition = _viewPosition;
	}
}

public class FixedUIScrollbar : UIScrollbar
{
	UserInterface userInterface;

	public FixedUIScrollbar(UserInterface userInterface)
	{
		this.userInterface = userInterface;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		UserInterface temp = UserInterface.ActiveInstance;
		UserInterface.ActiveInstance = userInterface;
		base.DrawSelf(spriteBatch);
		UserInterface.ActiveInstance = temp;
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		UserInterface temp = UserInterface.ActiveInstance;
		UserInterface.ActiveInstance = userInterface;
		base.LeftMouseDown(evt);
		UserInterface.ActiveInstance = temp;
	}
}
