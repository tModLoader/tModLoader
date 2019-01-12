using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.Config.UI
{
	public abstract class ConfigElement : UIElement
	{
		protected PropertyFieldWrapper memberInfo;
		protected object item;
		protected IList array;

		private Color backgroundColor; // TODO inherit parent object color?
		protected Func<string> _TextDisplayFunction;
		protected Func<string> _TooltipFunction;
		protected bool drawLabel = true;

		protected LabelAttribute labelAttribute;
		protected TooltipAttribute tooltipAttribute;
		protected BackgroundColorAttribute backgroundColorAttribute;
		protected RangeAttribute rangeAttribute;
		protected IncrementAttribute incrementAttribute;

		public ConfigElement(PropertyFieldWrapper memberInfo, object item, IList array)
		{
			Width.Set(0f, 1f);
			Height.Set(30f, 0f);
			this.memberInfo = memberInfo;
			this.item = item;
			this.array = array;
			this.backgroundColor = UICommon.defaultUIBlue;
			this._TextDisplayFunction = () => memberInfo.Name;
			labelAttribute = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, item, array);
			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label;
			}
			tooltipAttribute = ConfigManager.GetCustomAttribute<TooltipAttribute>(memberInfo, item, array);
			if (tooltipAttribute != null)
			{
				this._TooltipFunction = () => tooltipAttribute.Tooltip;
			}
			backgroundColorAttribute = ConfigManager.GetCustomAttribute<BackgroundColorAttribute>(memberInfo, item, array);
			if (backgroundColorAttribute != null)
			{
				backgroundColor = backgroundColorAttribute.color;
			}
			rangeAttribute = ConfigManager.GetCustomAttribute<RangeAttribute>(memberInfo, item, array);
			incrementAttribute = ConfigManager.GetCustomAttribute<IncrementAttribute>(memberInfo, item, array);
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
				UIModConfig.tooltip = _TooltipFunction();
				//string hoverText = _TooltipFunction(); // TODO: Fix, draw order prevents this from working correctly
				//float x = Main.fontMouseText.MeasureString(hoverText).X;
				//vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
				//if (vector.Y > (float)(Main.screenHeight - 30))
				//{
				//	vector.Y = (float)(Main.screenHeight - 30);
				//}
				//if (vector.X > (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16))
				//{
				//	vector.X = (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16);
				//}
				//Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, hoverText, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
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
}
