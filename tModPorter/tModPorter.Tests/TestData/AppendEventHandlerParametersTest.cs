using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

public class AppendEventHandlerParametersTest
{
	void Method(UIElement element, UIElement.DrawEvent drawEvent, UIElement.ElementEvent elementEvent) {
		element.OnDraw += e => { };
		element.OnDraw += (e) => { };
		element.OnDraw += (UIElement e) => { };
		element.OnDraw += delegate (UIElement e) { };
		element.OnDraw += delegate { };
		element.OnDraw += DrawHandler;
		element.OnDraw += drawEvent;
		element.OnDraw += elementEvent;
		element.OnDraw += (e, sb) => { };
		element.OnDraw -= e => { };
	}

	void DrawHandler(UIElement element) { }
}
