using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

public class AppendEventHandlerParametersTest
{
	UIElement.DrawEvent drawEventField = null;

	void Method(UIElement element, UIElement.DrawEvent drawEvent, UIElement.ElementEvent elementEvent) {
		element.OnDraw += e => { };
		element.OnDraw += (e) => { };
		element.OnDraw += (UIElement e) => { };
		element.OnDraw += delegate (UIElement e) { };
		element.OnDraw += delegate { };
		element.OnDraw += DrawHandler;
		element.OnDraw += NewDrawHandler;
		element.OnDraw += drawEvent;
		element.OnDraw += this.drawEventField;
		element.OnDraw += elementEvent;
		element.OnDraw += (e, sb) => { };
		element.OnDraw -= e => { };
	}

	void DrawHandler(UIElement element) { }

	void NewDrawHandler(UIElement element, SpriteBatch sb) { }
}
