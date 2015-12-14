using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UICycleImage : UIElement
	{
		private Texture2D texture;
		private int _drawWidth;
		private int _drawHeight;
		private int padding;
		private int textureOffsetX;
		private int textureOffsetY;
		private int currentState = 0;
		private int states;

		public UICycleImage(Texture2D texture, int states, int width, int height, int textureOffsetX, int textureOffsetY, int padding = 2)
		{
			this.texture = texture;
			this._drawWidth = width;
			this._drawHeight = height;
			this.textureOffsetX = textureOffsetX;
			this.textureOffsetY = textureOffsetY;
			this.Width.Set((float)width, 0f);
			this.Height.Set((float)height, 0f);
			this.states = states;
			this.padding = padding;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetDimensions();
			Point point = new Point(textureOffsetX, textureOffsetY + ((padding + _drawHeight) *currentState));
			Color color = base.IsMouseHovering ? Color.White : Color.Silver;
			spriteBatch.Draw(texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, this._drawWidth, this._drawHeight), new Rectangle?(new Rectangle(point.X, point.Y, this._drawWidth, this._drawHeight)), color);
		}

		public override void Click(UIMouseEvent evt)
		{
			currentState = (currentState + 1) % states;
			Interface.modBrowser.sortMode = Interface.modBrowser.sortMode.Next();
            base.Click(evt);
		}
	
	}
}
