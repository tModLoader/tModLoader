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
		_textElement?.SetText(_text);
		ResetScrollbar();
	}

	private void ResetScrollbar()
	{
		if (_scrollbar != null) {
			_scrollbar.ViewPosition = 0;
		}
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
			MinWidth = StyleDimension.Empty, // IsWrapped and ctor call InternalSetText, which assigns a MinWidth, this must be below IsWrapped. 
			TextOriginX = 0f,
		});
	}

	public void SetScrollbar(UIScrollbar scrollbar)
	{
		_scrollbar = scrollbar;
	}
}