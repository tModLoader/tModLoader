using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.GameContent.UI.Elements
{
	public partial class UIScrollbar : UIElement
	{
		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIScrollbar");
		}
	}

	public class FixedUIScrollbar : UIScrollbar
	{
		UserInterface userInterface;

		public FixedUIScrollbar(UserInterface userInterface) {
			this.userInterface = userInterface;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
}
