using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	abstract class UIModConfigItem : UIElement
	{
		private Color backgroundColor; // TODO inherit parent object color?
		protected Func<string> _TextDisplayFunction;
		protected Func<string> _TooltipFunction;
		protected PropertyFieldWrapper memberInfo;
		protected object item;
		protected bool drawLabel = true;

		public UIModConfigItem(PropertyFieldWrapper memberInfo, object item)
		{
			Width.Set(0f, 1f);
			Height.Set(0f, 1f);
			this.memberInfo = memberInfo;
			this.item = item;
			this.backgroundColor = UICommon.defaultUIBlue;
			this._TextDisplayFunction = () => memberInfo.Name;
			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label;
			}
			TooltipAttribute tta = (TooltipAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(TooltipAttribute));
			if (tta != null)
			{
				this._TooltipFunction = () => tta.tooltip;
			}

			// Class
			BackgroundColorAttribute bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(memberInfo.Type, typeof(BackgroundColorAttribute), true);
			if (bca != null)
			{
				backgroundColor = bca.color;
			}
			//if (array != null)
			//{
			//	bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(item.GetType(), typeof(BackgroundColorAttribute), true);
			//	if (bca != null)
			//	{
			//		backgroundColor = bca.color;
			//	}
			//}
			// Member
			bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(BackgroundColorAttribute));
			if (bca != null)
			{
				backgroundColor = bca.color;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			float settingsWidth = dimensions.Width + 1f;
			Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
			Vector2 baseScale = new Vector2(0.8f);
			Color color = IsMouseHovering ? Color.White : Color.White;
			if (!memberInfo.CanWrite)
				color = Color.Gray;
			//color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
			Color panelColor = base.IsMouseHovering ? this.backgroundColor : this.backgroundColor.MultiplyRGBA(new Color(180, 180, 180));
			Vector2 position = vector;
			DrawPanel2(spriteBatch, position, Main.settingsPanelTexture, settingsWidth, dimensions.Height, panelColor);
			if (drawLabel)
			{
				position.X += 8f;
				position.Y += 8f;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, this._TextDisplayFunction(), position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
			}
			if (IsMouseHovering && _TooltipFunction != null)
			{
				string hoverText = _TooltipFunction();
				float x = Main.fontMouseText.MeasureString(hoverText).X;
				vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
				if (vector.Y > (float)(Main.screenHeight - 30))
				{
					vector.Y = (float)(Main.screenHeight - 30);
				}
				if (vector.X > (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16))
				{
					vector.X = (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16);
				}
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, hoverText, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
			}
		}

		public static void DrawPanel2(SpriteBatch spriteBatch, Vector2 position, Texture2D texture, float width, float height, Color color)
		{
			// left edge
			//	spriteBatch.Draw(texture, position, new Rectangle(0, 0, 2, texture.Height), color);
			//	spriteBatch.Draw(texture, new Vector2(position.X + 2, position.Y), new Rectangle(2, 0, texture.Width - 4, texture.Height), color, 0f, Vector2.Zero, new Vector2((width - 4) / (texture.Width - 4), (height - 4) / (texture.Height - 4)), SpriteEffects.None, 0f);
			//	spriteBatch.Draw(texture, new Vector2(position.X + width - 2, position.Y), new Rectangle(texture.Width - 2, 0, 2, texture.Height), color);


			//width and height include border
			spriteBatch.Draw(texture, position + new Vector2(0, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, position + new Vector2(width - 2, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, position + new Vector2(2, 0), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, position + new Vector2(2, height - 2), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);

			spriteBatch.Draw(texture, position + new Vector2(2, 2), new Rectangle(2, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, position + new Vector2(2, 2 + ((height - 4) / 2)), new Rectangle(2, 16, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);
		}
	}

	internal class UIModConfigBooleanItem : UIModConfigItem
	{
		private Func<bool> _IsOnFunction;
		private Texture2D _toggleTexture;

		// TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
		public UIModConfigBooleanItem(PropertyFieldWrapper memberInfo, object modConfig, IList<bool> array = null, int index = -1) : base(memberInfo, modConfig)
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
				if(memberInfo.CanWrite)
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
