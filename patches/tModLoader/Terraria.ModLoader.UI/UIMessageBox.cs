using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using ReLogic.Graphics;

namespace Terraria.ModLoader.UI
{
	internal class UIMessageBox : UIPanel
	{
		private string text;
		protected UIScrollbar _scrollbar;
		private float height;
		private bool heightNeedsRecalculating;

		public UIMessageBox(string text)
		{
			this.text = text;
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
		}

		internal void SetText(string text)
		{
			this.text = text;
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle space = GetInnerDimensions();
			DynamicSpriteFont font = Main.fontMouseText;
			float position = 0f;
			if (this._scrollbar != null)
			{
				position = -this._scrollbar.GetValue();
			}
			float textHeight = font.MeasureString("A").Y;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				string drawString = line;
				bool notEnoughSpaceToDraw = false;
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
						notEnoughSpaceToDraw = true;
						break;
					}
					if (!notEnoughSpaceToDraw && position >= 0)
					{
						Utils.DrawBorderString(spriteBatch, drawString, new Vector2(space.X, space.Y + position), Color.White, 1f);
					}
					position += textHeight;
					drawString = remainder;
				}
				if (notEnoughSpaceToDraw)
				{
					break;
				}
			}
			this.Recalculate();
		}

		public override void RecalculateChildren()
		{
			base.RecalculateChildren();
			if (!heightNeedsRecalculating)
			{
				return;
			}
			CalculatedStyle space = GetInnerDimensions();
			if (space.Width <= 0 || space.Height <= 0)
			{
				return;
			}
			DynamicSpriteFont font = Main.fontMouseText;
			float position = 0f;
			float textHeight = font.MeasureString("A").Y;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				string drawString = line;
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
					position += textHeight;
					drawString = remainder;
				}
			}
			height = position;
			heightNeedsRecalculating = false;
		}

		public override void Recalculate()
		{
			base.Recalculate();
			this.UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			}
		}

		public void SetScrollbar(UIScrollbar scrollbar)
		{
			this._scrollbar = scrollbar;
			this.UpdateScrollbar();
			this.heightNeedsRecalculating = true;
		}

		private void UpdateScrollbar()
		{
			if (this._scrollbar == null)
			{
				return;
			}
			this._scrollbar.SetView(base.GetInnerDimensions().Height, this.height);
		}
	}
}
