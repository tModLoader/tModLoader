using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	internal class UIMessageBox : UIPanel
	{
		protected UIScrollbar Scrollbar;

		private string _text;
		private float _height;
		private bool _heightNeedsRecalculating;
		private TextSnippet[][] _textSnippets;

		public UIMessageBox(string text) {
			SetText(text);
		}

		public override void OnActivate() {
			base.OnActivate();
			_heightNeedsRecalculating = true;
		}

		internal void SetText(string text) {
			_text = text;
			ResetScrollbar();
		}

		private void ResetScrollbar() {
			if (Scrollbar != null) {
				Scrollbar.ViewPosition = 0;
				_heightNeedsRecalculating = true;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle space = GetInnerDimensions();
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			
			float textHeight = font.MeasureString("A").Y;
			var drawPosition = new Vector2(space.X, space.Y);

			if (Scrollbar != null) {
				drawPosition.Y -= Scrollbar.GetValue();
			}

			if (_textSnippets != null) {
				for (int y = 0; y < _textSnippets.Length; y++) {
					var lineSnippets = _textSnippets[y];

					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, lineSnippets, drawPosition, 0f, Vector2.Zero, Vector2.One, out _);

					drawPosition.Y += textHeight;
				}
			}

			Recalculate();
		}

		public override void RecalculateChildren() {
			base.RecalculateChildren();

			_textSnippets = null;

			if (!_heightNeedsRecalculating && _textSnippets != null) {
				return;
			}

			CalculatedStyle space = GetInnerDimensions();

			if (space.Width <= 0 || space.Height <= 0) {
				return;
			}

			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float textHeight = font.MeasureString("A").Y;
			var wrapResult = Utils.WordwrapStringSmart(_text, Color.White, font, (int)MathF.Floor(space.Width), (int)MathF.Floor(space.Height / textHeight));

			_textSnippets = new TextSnippet[wrapResult.Count][];

			for (int y = 0; y < _textSnippets.Length; y++) {
				var list = wrapResult[y];
				var array = _textSnippets[y] = new TextSnippet[list.Count];

				list.CopyTo(array);
				ChatManager.ConvertNormalSnippets(array); // Disables blinking.
			}

			_height = _textSnippets.Length * textHeight;
			_heightNeedsRecalculating = false;
		}

		public override void Recalculate() {
			base.Recalculate();
			UpdateScrollbar();
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIMessageBox");
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (Scrollbar != null) {
				Scrollbar.ViewPosition -= evt.ScrollWheelValue;
			}
		}

		public void SetScrollbar(UIScrollbar scrollbar) {
			Scrollbar = scrollbar;
			UpdateScrollbar();
			_heightNeedsRecalculating = true;
		}

		private void UpdateScrollbar() {
			Scrollbar?.SetView(GetInnerDimensions().Height, _height);
		}
	}
}
