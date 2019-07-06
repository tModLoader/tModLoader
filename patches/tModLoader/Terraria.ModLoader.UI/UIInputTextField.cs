using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		public event EventHandler OnTab;

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

	internal class UIFocusInputTextField : UIElement
	{
		internal bool focused = false;
		private string hintText;
		internal string currentString = "";
		private int textBlinkerCount;
		private int textBlinkerState;
		public bool UnfocusOnTab { get; internal set; } = false;

		public delegate void EventHandler(Object sender, EventArgs e);

		public event EventHandler OnTextChange;
		public event EventHandler OnUnfocus;
		public event EventHandler OnTab;

		public UIFocusInputTextField(string hintText)
		{
			this.hintText = hintText;
		}

		public void SetText(string text)
		{
			if (text == null)
				text = "";
			if (currentString != text)
			{
				currentString = text;
				OnTextChange?.Invoke(this, new EventArgs());
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			Main.clrInput();
			focused = true;
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (!ContainsPoint(MousePosition) && Main.mouseLeft) // TODO: && focused maybe?
			{
				focused = false;
				OnUnfocus?.Invoke(this, new EventArgs());
			}
			base.Update(gameTime);
		}
		private static bool JustPressed(Keys key) {
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);

			if (focused)
			{
				GameInput.PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (!newString.Equals(currentString))
				{
					currentString = newString;
					OnTextChange?.Invoke(this, new EventArgs());
				}
				else
				{
					currentString = newString;
				}
				if (JustPressed(Keys.Tab)) {
					if (UnfocusOnTab) {
						focused = false;
						OnUnfocus?.Invoke(this, new EventArgs());
					}
					OnTab?.Invoke(this, new EventArgs());
				}
				if (++textBlinkerCount >= 20)
				{
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
			}
			string displayString = currentString;
			if (this.textBlinkerState == 1 && focused)
			{
				displayString = displayString + "|";
			}
			CalculatedStyle space = base.GetDimensions();
			if (currentString.Length == 0 && !focused)
			{
				Utils.DrawBorderString(spriteBatch, hintText, new Vector2(space.X, space.Y), Color.Gray, 1f);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White, 1f);
			}
		}
	}
}