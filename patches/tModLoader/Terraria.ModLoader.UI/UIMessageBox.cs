using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIMessageBox : UIPanel
	{
		private string text;

		public UIMessageBox(string text)
		{
			this.text = text;
		}

		internal void SetText(string text)
		{
			this.text = text;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle space = GetInnerDimensions();
			SpriteFont font = Main.fontMouseText;
			float position = 0f;
			float textHeight = font.MeasureString("A").Y;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				string drawString = line;
				bool flag = false;
				if (drawString.Length == 0)
				{
					position += textHeight;
				}
				while (drawString.Length > 0)
				{
					string remainder = "";
					while (font.MeasureString(drawString).X > space.Width)
					{
						remainder = drawString[drawString.Length - 1] + remainder;
						drawString = drawString.Substring(0, drawString.Length - 1);
					}
					if (remainder.Length > 0)
					{
						int index = drawString.LastIndexOf(' ');
						if (index >= 0)
						{
							remainder = drawString.Substring(index + 1) + remainder;
							drawString = drawString.Substring(0, index);
						}
					}
					if (position + textHeight > space.Height)
					{
						flag = true;
						break;
					}
					Utils.DrawBorderString(spriteBatch, drawString, new Vector2(space.X, space.Y + position), Color.White, 1f);
					position += textHeight;
					drawString = remainder;
				}
				if (flag)
				{
					break;
				}
			}
		}
	}
}
