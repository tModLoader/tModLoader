using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIMessageBox : UIPanel
{
	string _text;
	UIScrollbar _scrollbar;
	UIList _list;
	UIText _textElement;

	public UIMessageBox(string text)
	{
		_text = text;
	}

	public void SetText(string text)
	{
		_text = text;
	}

	public override void OnInitialize()
	{
		Append(_list = new UIList() {
			Left = StyleDimension.Empty,
			Top = StyleDimension.Empty,
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
		});
		_list.SetScrollbar(_scrollbar);

		_list.Add(_textElement = new UIText(_text) {
			Width = StyleDimension.Fill,
			IsWrapped = true,
		});
	}

	public override void Update(GameTime gameTime)
	{
		_textElement.SetText(_text);

		base.Update(gameTime);
	}

	public void SetScrollbar(UIScrollbar scrollbar)
	{
		_scrollbar = scrollbar;
	}
}