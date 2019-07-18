using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Reflection;
using Terraria.Graphics;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.Config.UI
{
	public abstract class ConfigElement<T> : ConfigElement
	{
		protected virtual T Value {
			get => (T)GetObject();
			set => SetObject(value);
		}
	}

	public abstract class ConfigElement : UIElement
	{
		protected Texture2D playTexture = TextureManager.Load("Images/UI/ButtonPlay");
		protected Texture2D deleteTexture = TextureManager.Load("Images/UI/ButtonDelete");
		protected Texture2D plusTexture = UICommon.ButtonPlusTexture;
		//protected Texture2D upArrowTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png"));
		//protected Texture2D downArrowTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
		protected Texture2D upDownTexture = UICommon.ButtonUpDownTexture;
		protected Texture2D collapsedTexture = UICommon.ButtonCollapsedTexture;
		protected Texture2D expandedTexture = UICommon.ButtonExpandedTexture;

		// Provides access to the field/property contained in the item
		protected PropertyFieldWrapper memberInfo;
		// The object that contains the memberInfo. This is usually a ModConfig instance or an object instance contained within a ModConfig instance.
		protected object item;
		// If non-null, the memberInfo actually referes to the collection containing this item and array and index need to be used to assign this data
		protected IList list;
		public int index;

		private Color backgroundColor; // TODO inherit parent object color?
		protected Func<string> TextDisplayFunction;
		protected Func<string> TooltipFunction;
		protected bool drawLabel = true;

		protected LabelAttribute labelAttribute;
		protected TooltipAttribute tooltipAttribute;
		protected BackgroundColorAttribute backgroundColorAttribute;
		protected RangeAttribute rangeAttribute;
		protected IncrementAttribute incrementAttribute;

		public ConfigElement()
		{
			Width.Set(0f, 1f);
			Height.Set(30f, 0f);
		}

		/// <summary>
		/// Bind must always be called after the ctor and serves to facilitate a convenient inheritance workflow for custom ConfigElemets from mods. 
		/// </summary>
		public void Bind(PropertyFieldWrapper memberInfo, object item, IList array, int index) {
			this.memberInfo = memberInfo;
			this.item = item;
			this.list = array;
			this.index = index;
			this.backgroundColor = UICommon.DefaultUIBlue;
		}

		public virtual void OnBind() {
			TextDisplayFunction = () => memberInfo.Name;
			labelAttribute = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, item, list);
			if (labelAttribute != null) {
				TextDisplayFunction = () => labelAttribute.Label;
			}
			tooltipAttribute = ConfigManager.GetCustomAttribute<TooltipAttribute>(memberInfo, item, list);
			if (tooltipAttribute != null) {
				this.TooltipFunction = () => tooltipAttribute.Tooltip;
			}
			backgroundColorAttribute = ConfigManager.GetCustomAttribute<BackgroundColorAttribute>(memberInfo, item, list);
			if (backgroundColorAttribute != null) {
				backgroundColor = backgroundColorAttribute.color;
			}
			rangeAttribute = ConfigManager.GetCustomAttribute<RangeAttribute>(memberInfo, item, list);
			incrementAttribute = ConfigManager.GetCustomAttribute<IncrementAttribute>(memberInfo, item, list);
		}

		protected virtual void SetObject(object value) {
			if (list != null) {
				list[index] = value;
				Interface.modConfig.SetPendingChanges();
				return;
			}
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, value);
			Interface.modConfig.SetPendingChanges();
		}
		protected virtual object GetObject() {
			if (list != null)
				return list[index];
			return memberInfo.GetValue(item);
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
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, TextDisplayFunction(), position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
			}
			if (IsMouseHovering && TooltipFunction != null)
			{
				UIModConfig.tooltip = TooltipFunction();
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

			//if (IsMouseHovering) {
			//	Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Green * 0.6f);
			//}
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

	internal class HeaderElement : UIElement
	{
		string header;
		public HeaderElement(string header) {
			this.header = header;
			Width.Set(0f, 1f);
			Height.Set(30f, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			float settingsWidth = dimensions.Width + 1f;
			Vector2 position = new Vector2(dimensions.X, dimensions.Y) + new Vector2(8);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)dimensions.X + 10, (int)dimensions.Y + (int)dimensions.Height - 2, (int)dimensions.Width - 20, 1), Color.LightGray);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, header, position, Color.White, 0f, Vector2.Zero, new Vector2(1f), settingsWidth, 2f);
		}
	}
}
