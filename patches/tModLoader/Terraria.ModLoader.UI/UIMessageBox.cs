using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIMessageBox : UIPanel
	{
		private string text;
		protected UIScrollbar _scrollbar;
		private float height;
		private bool heightNeedsRecalculating;
		private List<Tuple<string, float>> drawtexts = new List<Tuple<string, float>>();

		public UIMessageBox(string text) {
			this.text = text;
			if (_scrollbar != null) {
				_scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
		}

		public override void OnActivate() {
			base.OnActivate();
			heightNeedsRecalculating = true;
		}

		internal void SetText(string text) {
			this.text = text;
			if (_scrollbar != null) {
				_scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle space = GetInnerDimensions();
			DynamicSpriteFont font = Main.fontMouseText;
			float position = 0f;
			if (_scrollbar != null) {
				position = -_scrollbar.GetValue();
			}
			foreach (var drawtext in drawtexts) {
				if (position + drawtext.Item2 > space.Height)
					break;
				if (position >= 0)
					Utils.DrawBorderString(spriteBatch, drawtext.Item1, new Vector2(space.X, space.Y + position), Color.White, 1f);
				position += drawtext.Item2;
			}
			this.Recalculate();
		}

		public override void RecalculateChildren() {
			base.RecalculateChildren();
			if (!heightNeedsRecalculating) {
				return;
			}
			CalculatedStyle space = GetInnerDimensions();
			if (space.Width <= 0 || space.Height <= 0) {
				return;
			}
			DynamicSpriteFont font = Main.fontMouseText;
			drawtexts.Clear();
			float position = 0f;
			float textHeight = font.MeasureString("A").Y;
			foreach (string line in text.Split('\n')) {
				string drawString = line;
				do {
					string remainder = "";
					while (font.MeasureString(drawString).X > space.Width) {
						remainder = drawString[drawString.Length - 1] + remainder;
						drawString = drawString.Substring(0, drawString.Length - 1);
					}
					if (remainder.Length > 0) {
						int index = drawString.LastIndexOf(' ');
						if (index >= 0) {
							remainder = drawString.Substring(index + 1) + remainder;
							drawString = drawString.Substring(0, index);
						}
					}
					drawtexts.Add(new Tuple<string, float>(drawString, textHeight));
					position += textHeight;
					drawString = remainder;
				}
				while (drawString.Length > 0);
			}
			height = position;
			heightNeedsRecalculating = false;
		}

		public override void Recalculate() {
			base.Recalculate();
			UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (_scrollbar != null) {
				_scrollbar.ViewPosition -= evt.ScrollWheelValue;
			}
		}

		public void SetScrollbar(UIScrollbar scrollbar) {
			_scrollbar = scrollbar;
			UpdateScrollbar();
			heightNeedsRecalculating = true;
		}

		private void UpdateScrollbar() {
			_scrollbar?.SetView(base.GetInnerDimensions().Height, this.height);
		}
	}
}
