using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.GameContent.UI.Elements;

public partial class UIScrollbar : UIElement
{
	public float ViewSize => _viewSize;
	public float MaxViewSize => _maxViewSize;

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (IsMouseHovering)
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIList");
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
