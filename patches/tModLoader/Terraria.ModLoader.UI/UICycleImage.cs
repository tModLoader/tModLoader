using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public class UICycleImage : UIElement
	{
		private readonly Texture2D _texture;
		private readonly int _padding;
		private readonly int _textureOffsetX;
		private readonly int _textureOffsetY;
		private readonly int _states;

		public event EventHandler OnStateChanged;

		private int _currentState;
		public int CurrentState {
			get => _currentState;
			set {
				if (value != _currentState) {
					_currentState = value;
					OnStateChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		protected int DrawHeight => (int)Height.Pixels;
		protected int DrawWidth => (int)Width.Pixels;
		
		public UICycleImage(Texture2D texture, int states, int width, int height, int textureOffsetX, int textureOffsetY, int padding = 2) {
			_texture = texture;
			_textureOffsetX = textureOffsetX;
			_textureOffsetY = textureOffsetY;
			Width.Pixels = width;
			Height.Pixels = height;
			_states = states;
			_padding = padding;
		}
		
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dimensions = GetDimensions();
			Point point = new Point(_textureOffsetX, _textureOffsetY + (_padding + DrawHeight) * _currentState);
			Color color = IsMouseHovering ? Color.White : Color.Silver;
			spriteBatch.Draw(_texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, DrawWidth, DrawHeight), new Rectangle(point.X, point.Y, DrawWidth, DrawHeight), color);
		}

		public override void Click(UIMouseEvent evt) {
			CurrentState = (_currentState + 1) % _states;
			base.Click(evt);
		}

		public override void RightClick(UIMouseEvent evt) {
			CurrentState = (_currentState + _states - 1) % _states;
			base.RightClick(evt);
		}

		internal void SetCurrentState(int sortMode) {
			CurrentState = sortMode;
		}
	}
}
