using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

public class AppendEventHandlerParametersTest
{
	UIElement.DrawEvent drawEventField = null;

	void Method(UIElement element, UIElement.DrawEvent drawEvent, UIElement.ElementEvent elementEvent) {
		element.OnDraw += (e, sb) => { };
		element.OnDraw += (e, sb) => { };
		element.OnDraw += (UIElement e, SpriteBatch sb) => { };
		element.OnDraw += delegate (UIElement e, SpriteBatch sb) { };
		element.OnDraw += delegate { };
#if COMPILE_ERROR
		element.OnDraw += DrawHandler/* tModPorter Suggestion: Handler parameters are now (UIElement affectedElement, SpriteBatch sb) */;
#endif
		element.OnDraw += NewDrawHandler;
		element.OnDraw += drawEvent;
		element.OnDraw += this.drawEventField;
#if COMPILE_ERROR
		element.OnDraw += elementEvent/* tModPorter Suggestion: Handler parameters are now (UIElement affectedElement, SpriteBatch sb) */;
#endif
		element.OnDraw += (e, sb) => { };
		element.OnDraw -= (e, sb) => { };
	}

	void DrawHandler(UIElement element) { }

	void NewDrawHandler(UIElement element, SpriteBatch sb) { }
}