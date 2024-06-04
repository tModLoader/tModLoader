using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria.Initializers;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIInputTextField : UIElement
{
	private readonly string _hintText;
	private string _currentString = string.Empty;
	private int _textBlinkerCount;

	public string Text {
		get => _currentString;
		set {
			if (_currentString != value) {
				_currentString = value;
				OnTextChange?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public delegate void EventHandler(object sender, EventArgs e);
	public event EventHandler OnTextChange;

	public UIInputTextField(string hintText)
	{
		_hintText = hintText;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		GameInput.PlayerInput.WritingText = true;
		Main.instance.HandleIME();
		string newString = Main.GetInputText(_currentString);
		if (newString != _currentString) {
			_currentString = newString;
			OnTextChange?.Invoke(this, EventArgs.Empty);
		}

		string displayString = _currentString;
		if (++_textBlinkerCount / 20 % 2 == 0)
			displayString += "|";

		CalculatedStyle space = GetDimensions();
		if (_currentString.Length == 0) {
			Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(space.X, space.Y), Color.Gray);
		}
		else {
			Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White);
		}
	}
}