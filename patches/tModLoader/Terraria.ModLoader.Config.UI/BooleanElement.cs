using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class BooleanElement : ConfigElement<bool>
	{
		private Texture2D _toggleTexture;

		// TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
		public override void OnBind() {
			base.OnBind();
			_toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");

			OnClick += (ev, v) => Value = !Value;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			Rectangle sourceRectangle = new Rectangle(Value ? ((_toggleTexture.Width - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width - 2) / 2, _toggleTexture.Height);
			Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
			spriteBatch.Draw(_toggleTexture, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}
	}
}
