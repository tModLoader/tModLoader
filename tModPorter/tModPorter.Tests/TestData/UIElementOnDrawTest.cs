using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

public class UIElementOnDrawTest
{
	void Method(UIElement element) {
		element.OnDraw += e => { };
		element.OnDraw += (e) => { };
		element.OnDraw += (UIElement e) => { };
		element.OnDraw += delegate (UIElement e) { };
		element.OnDraw += delegate { };
		element.OnDraw += DrawHandler;
		element.OnDraw += (e, sb) => { };
		element.OnDraw -= e => { };
	}

	void DrawHandler(UIElement element) { }
}
