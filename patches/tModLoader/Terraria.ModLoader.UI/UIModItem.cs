using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIModItem : UIPanel
	{
		private TmodFile mod;
		private Texture2D dividerTexture;
		private Texture2D innerPanelTexture;
		private UIText modName;
		internal bool enabled;
		BuildProperties properties;
		UITextPanel button2;
		UIHoverImage keyImage;

		public UIModItem(TmodFile mod)
		{
			this.mod = mod;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			//base.OnClick += new UIElement.MouseEvent(this.ToggleEnabled);
			properties = BuildProperties.ReadModFile(mod);
			string text = properties.displayName.Length > 0 ? properties.displayName : mod.name;
			text += " " + mod.version;
			if (properties.author.Length > 0)
			{
				text += " - by " + properties.author;
			}
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			this.enabled = ModLoader.IsEnabled(mod);
			UITextPanel button = new UITextPanel("More info", 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(430f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(this.Moreinfo);
			base.Append(button);
			button2 = new UITextPanel(this.enabled ? "Click to Disable" : "Click to Enable", 1f, false);
			button2.Width.Set(100f, 0f);
			button2.Height.Set(30f, 0f);
			button2.Left.Set(275f, 0f);
			button2.Top.Set(40f, 0f);
			button2.PaddingTop -= 2f;
			button2.PaddingBottom -= 2f;
			button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button2.OnClick += new UIElement.MouseEvent(this.ToggleEnabled);
			base.Append(button2);
			if (mod.validModBrowserSignature)
			{
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.GoldenKey], "This mod originated from the Mod Browser");
				keyImage.Left.Set(-20, 1f);
				base.Append(keyImage);
			}
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
		{
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			string text = this.enabled ? "Enabled" : "Disabled";
			Color color = this.enabled ? Color.Green : Color.Red;
			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + 10f, innerDimensions.Y + 45f);
			this.DrawPanel(spriteBatch, drawPos, 100f);
			this.DrawEnabledText(spriteBatch, drawPos + new Vector2(15f, 5f));
			if (this.enabled != ModLoader.ModLoaded(mod.name))
			{
				drawPos += new Vector2(120f, 5f);
				Utils.DrawBorderString(spriteBatch, "Reload Required", drawPos, Color.White, 1f, 0f, 0f, -1);
			}
			//string text = this.enabled ? "Click to Disable" : "Click to Enable";
			//drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 150f, innerDimensions.Y + 50f);
			//Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			this.BackgroundColor = new Color(73, 94, 171);
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
		}

		internal void ToggleEnabled(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			this.enabled = !this.enabled;
			button2.SetText(this.enabled ? "Click to Disable" : "Click to Enable", 1f, false);
			ModLoader.SetModActive(this.mod, this.enabled);
		}

		internal void Moreinfo(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.SetModName(properties.displayName);
			Interface.modInfo.SetModInfo(properties.description);
			Interface.modInfo.SetGotoMenu(Interface.modsMenuID);
			Interface.modInfo.SetURL(properties.homepage);
			Main.menuMode = Interface.modInfoID;
		}
	}
}
