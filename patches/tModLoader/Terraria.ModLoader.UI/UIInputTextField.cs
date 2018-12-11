using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInputTextField : UIElement
	{
		private string hintText;
		private string currentString = "";
		private int textBlinkerCount;

		public string Text {
			get => currentString;
			set {
				if (currentString != value) {
					currentString = value;
					OnTextChange?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public delegate void EventHandler(object sender, EventArgs e);
		public event EventHandler OnTextChange;

		public UIInputTextField(string hintText) {
			this.hintText = hintText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			GameInput.PlayerInput.WritingText = true;
			Main.instance.HandleIME();
			string newString = Main.GetInputText(currentString);
			if (newString != currentString) {
				currentString = newString;
				OnTextChange(this, EventArgs.Empty);
			}
			
			string displayString = currentString;
			if ((++textBlinkerCount) / 20 % 2 == 0)
				displayString += "|";
			
			CalculatedStyle space = GetDimensions();
			if (currentString.Length == 0) {
				Utils.DrawBorderString(spriteBatch, hintText, new Vector2(space.X, space.Y), Color.Gray, 1f);
			}
			else {
				Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White, 1f);
			}
		}
	}
}