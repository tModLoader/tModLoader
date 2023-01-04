using Microsoft.Xna.Framework;
using System;

#nullable enable

namespace Terraria.UI;

partial class UserInterface
{
	private InputPointerCache MiddleMouse = new InputPointerCache {
		MouseDownEvent = (element, evt) => element.MiddleMouseDown(evt),
		MouseUpEvent = (element, evt) => element.MiddleMouseUp(evt),
		ClickEvent = (element, evt) => element.MiddleClick(evt),
		DoubleClickEvent = (element, evt) => element.MiddleDoubleClick(evt)
	};
	private InputPointerCache XButton1Mouse = new InputPointerCache {
		MouseDownEvent = (element, evt) => element.XButton1MouseDown(evt),
		MouseUpEvent = (element, evt) => element.XButton1MouseUp(evt),
		ClickEvent = (element, evt) => element.XButton1Click(evt),
		DoubleClickEvent = (element, evt) => element.XButton1DoubleClick(evt)
	};
	private InputPointerCache XButton2Mouse = new InputPointerCache {
		MouseDownEvent = (element, evt) => element.XButton2MouseDown(evt),
		MouseUpEvent = (element, evt) => element.XButton2MouseUp(evt),
		ClickEvent = (element, evt) => element.XButton2Click(evt),
		DoubleClickEvent = (element, evt) => element.XButton2DoubleClick(evt)
	};
}
