using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public class UICycleImage : UIElement
	{
		private readonly Texture2D texture;
		private readonly int _drawWidth;
		private readonly int _drawHeight;
		private readonly int padding;
		private readonly int textureOffsetX;
		private readonly int textureOffsetY;
		private readonly int states;

		public event EventHandler OnStateChanged;

		private int currentState;
		public int CurrentState {
			get => currentState;
			set {
				if (value != currentState) {
					currentState = value;
					OnStateChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public UICycleImage(Texture2D texture, int states, int width, int height, int textureOffsetX, int textureOffsetY, int padding = 2) {
			this.texture = texture;
			_drawWidth = width;
			_drawHeight = height;
			this.textureOffsetX = textureOffsetX;
			this.textureOffsetY = textureOffsetY;
			Width.Pixels = width;
			Height.Pixels = height;
			this.states = states;
			this.padding = padding;
		}

		// TODO could be cleaned up, perhaps a better Draw overload exists, also can we not just use the actual width of the element, or the Width set by the style rather than _drawWidth
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dimensions = GetDimensions();
			Point point = new Point(textureOffsetX, textureOffsetY + ((padding + _drawHeight) * currentState));
			Color color = IsMouseHovering ? Color.White : Color.Silver;
			spriteBatch.Draw(texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, _drawWidth, _drawHeight), new Rectangle(point.X, point.Y, _drawWidth, _drawHeight), color);
		}

		public override void Click(UIMouseEvent evt) {
			CurrentState = (currentState + 1) % states;
			base.Click(evt);
		}

		public override void RightClick(UIMouseEvent evt) {
			CurrentState = (currentState + states - 1) % states;
			base.RightClick(evt);
		}

		internal void SetCurrentState(int sortMode) {
			CurrentState = sortMode;
		}
	}
}
