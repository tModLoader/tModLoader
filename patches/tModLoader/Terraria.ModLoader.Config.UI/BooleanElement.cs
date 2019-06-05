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
		public IList<bool> boolList;

		// TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
		public override void OnBind() {
			base.OnBind();
			boolList = (IList<bool>)list;
			_toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");

			if (boolList != null) {
				_IsOnFunction = () => boolList[index];
				OnClick += (ev, v) => {
					boolList[index] = !boolList[index];
					Interface.modConfig.SetPendingChanges();
				};
			}
			else {
				_IsOnFunction = () => (bool)memberInfo.GetValue(item);
				if (memberInfo.CanWrite)
					OnClick += (ev, v) => {
						memberInfo.SetValue(item, !(bool)memberInfo.GetValue(item));
						Interface.modConfig.SetPendingChanges();
					};
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			Rectangle sourceRectangle = new Rectangle(_IsOnFunction() ? ((_toggleTexture.Width - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width - 2) / 2, _toggleTexture.Height);
			Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
			spriteBatch.Draw(_toggleTexture, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}
	}
}
