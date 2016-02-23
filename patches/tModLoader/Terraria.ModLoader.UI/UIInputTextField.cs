using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Windows.Forms;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Terraria.ModLoader.UI
{
	internal class UIInputTextField : UIElement
	{
		private string hintText;
		internal string currentString = "";
		private int textBlinkerCount;
		private int textBlinkerState;
		private static KeyboardState inputText;
		private static KeyboardState oldInputText;
		private static bool inputTextEscape;
		private static bool inputTextEnter;
		private static int backSpaceCount;

		public delegate void EventHandler(Object sender, EventArgs e);

		public event EventHandler OnTextChange;

		public UIInputTextField(string hintText)
		{
			this.hintText = hintText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			String newString = GetInputText(currentString);
			if (!newString.Equals(currentString))
			{
				currentString = newString;
				OnTextChange(this, new EventArgs());
			}
			else
			{
				currentString = newString;
			}
			if (++textBlinkerCount >= 20)
			{
				textBlinkerState = (textBlinkerState + 1) % 2;
				textBlinkerCount = 0;
			}
			String displayString = currentString;
			if (this.textBlinkerState == 1)
			{
				displayString = displayString + "|";
			}
			CalculatedStyle space = base.GetDimensions();
			if (currentString.Length == 0)
			{
				Utils.DrawBorderString(spriteBatch, hintText, new Vector2(space.X, space.Y), Color.Gray, 1f);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White, 1f);
			}
		}

		public static string GetInputText(string oldString)
		{
			if (!Main.hasFocus)
			{
				return oldString;
			}
			inputTextEnter = false;
			inputTextEscape = false;
			string text = oldString;
			string newKeys = "";
			if (text == null)
			{
				text = "";
			}
			bool flag = false;
			if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
			{
				if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z))
				{
					text = "";
				}
				else if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X))
				{
					PlatformUtilties.SetClipboard(oldString);
					text = "";
				}
				else if ((inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C)) || (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert)))
				{
					PlatformUtilties.SetClipboard(oldString);
				}
				else if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.V) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.V))
				{
					newKeys += PlatformUtilties.GetClipboard();
				}
			}
			else
			{
				if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
				{
					if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
					{
						Thread thread = new Thread((ThreadStart)delegate
							{
								if (oldString.Length > 0)
								{
									Clipboard.SetText(oldString);
								}
							});
						thread.SetApartmentState(ApartmentState.STA);
						thread.Start();
						while (thread.IsAlive)
						{
						}
						text = "";
					}
					if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert) && !oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
					{
						Thread thread2 = new Thread((ThreadStart)delegate
							{
								string text2 = Clipboard.GetText();
								for (int l = 0; l < text2.Length; l++)
								{
									if (text2[l] < ' ' || text2[l] == '\u007f')
									{
										text2 = text2.Replace(string.Concat(text2[l--]), "");
									}
								}
								newKeys += text2;
							});
						thread2.SetApartmentState(ApartmentState.STA);
						thread2.Start();
						while (thread2.IsAlive)
						{
						}
					}
				}
				for (int i = 0; i < Main.keyCount; i++)
				{
					int num = Main.keyInt[i];
					string str = Main.keyString[i];
					if (num == 13)
					{
						inputTextEnter = true;
					}
					else if (num == 27)
					{
						inputTextEscape = true;
					}
					else if (num >= 32 && num != 127)
					{
						newKeys += str;
					}
				}
			}
			Main.keyCount = 0;
			text += newKeys;
			oldInputText = inputText;
			inputText = Keyboard.GetState();
			Microsoft.Xna.Framework.Input.Keys[] pressedKeys = inputText.GetPressedKeys();
			Microsoft.Xna.Framework.Input.Keys[] pressedKeys2 = oldInputText.GetPressedKeys();
			if (inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back) && oldInputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back))
			{
				if (backSpaceCount == 0)
				{
					backSpaceCount = 7;
					flag = true;
				}
				backSpaceCount--;
			}
			else
			{
				backSpaceCount = 15;
			}
			for (int j = 0; j < pressedKeys.Length; j++)
			{
				bool flag2 = true;
				for (int k = 0; k < pressedKeys2.Length; k++)
				{
					if (pressedKeys[j] == pressedKeys2[k])
					{
						flag2 = false;
					}
				}
				string a = string.Concat(pressedKeys[j]);
				if (a == "Back" && (flag2 || flag) && text.Length > 0)
				{
					TextSnippet[] array = ChatManager.ParseMessage(text, Microsoft.Xna.Framework.Color.White);
					if (array[array.Length - 1].DeleteWhole)
					{
						text = text.Substring(0, text.Length - array[array.Length - 1].TextOriginal.Length);
					}
					else
					{
						text = text.Substring(0, text.Length - 1);
					}
				}
			}
			return text;
		}
	}
}