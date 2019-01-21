using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class BooleanElement : ConfigElement
	{
		private Func<bool> _IsOnFunction;
		private Texture2D _toggleTexture;

		// TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
		public BooleanElement(PropertyFieldWrapper memberInfo, object modConfig, IList<bool> array = null, int index = -1) : base(memberInfo, modConfig, (IList)array)
		{
			this._toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");

			if (array != null)
			{
				_IsOnFunction = () => array[index];
				this.OnClick += (ev, v) =>
				{
					array[index] = !array[index];
					Interface.modConfig.SetPendingChanges();
				};
			}
			else
			{
				this._IsOnFunction = () => (bool)memberInfo.GetValue(modConfig);
				if (memberInfo.CanWrite)
					this.OnClick += (ev, v) =>
					{
						memberInfo.SetValue(modConfig, !(bool)memberInfo.GetValue(modConfig));
						Interface.modConfig.SetPendingChanges();
					};
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			Rectangle sourceRectangle = new Rectangle(this._IsOnFunction() ? ((this._toggleTexture.Width - 2) / 2 + 2) : 0, 0, (this._toggleTexture.Width - 2) / 2, this._toggleTexture.Height);
			Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
			spriteBatch.Draw(this._toggleTexture, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}
	}
}
