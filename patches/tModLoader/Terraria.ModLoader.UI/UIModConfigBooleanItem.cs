using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	public class UIModConfigBooleanItem : UIElement
	{
		private Color _color;
		private Func<string> _TextDisplayFunction;
		private Func<bool> _IsOnFunction;
		private Texture2D _toggleTexture;

		public UIModConfigBooleanItem(Func<string> getText, Func<bool> getStatus, Color color)
		{
			this._color = color;
			this._toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");
			Func<string> arg_41_1;
			if (getText == null)
			{
				arg_41_1 = (() => "???");
			}
			else
			{
				arg_41_1 = getText;
			}
			this._TextDisplayFunction = arg_41_1;
			Func<bool> arg_6A_1;
			if (getStatus == null)
			{
				arg_6A_1 = (() => false);
			}
			else
			{
				arg_6A_1 = getStatus;
			}
			this._IsOnFunction = arg_6A_1;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			float num = 6f;
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			float num2 = dimensions.Width + 1f;
			Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
			bool flag = false;
			Vector2 baseScale = new Vector2(0.8f);
			Color color = flag ? Color.Gold : (base.IsMouseHovering ? Color.White : Color.Silver);
			color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 0.5f : 0f);
			Color color2 = base.IsMouseHovering ? this._color : this._color.MultiplyRGBA(new Color(180, 180, 180));
			Vector2 position = vector;
			Utils.DrawSettingsPanel(spriteBatch, position, num2, color2);
			position.X += 8f;
			position.Y += 2f + num;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, this._TextDisplayFunction(), position, color, 0f, Vector2.Zero, baseScale, num2, 2f);
			position.X -= 17f;
			Rectangle value = new Rectangle(this._IsOnFunction() ? ((this._toggleTexture.Width - 2) / 2 + 2) : 0, 0, (this._toggleTexture.Width - 2) / 2, this._toggleTexture.Height);
			Vector2 vector2 = new Vector2((float)value.Width, 0f);
			position = new Vector2(dimensions.X + dimensions.Width - vector2.X - 10f, dimensions.Y + 2f + num);
			spriteBatch.Draw(this._toggleTexture, position, new Rectangle?(value), Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}
	}
}
