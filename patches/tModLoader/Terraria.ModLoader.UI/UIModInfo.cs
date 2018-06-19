using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.ModLoader.IO;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIModInfo : UIState
	{
		internal UIElement uIElement;
		public UIMessageBox modInfo;
		public UITextPanel<string> uITextPanel;
		internal UITextPanel<string> modHomepageButton;
		internal UITextPanel<string> extractButton;
		internal UITextPanel<string> deleteButton;
		private int gotoMenu = 0;
		private LocalMod localMod;
		private string url = "";
		private string info = "";
		private string modDisplayName = "";

		public override void OnInitialize()
		{
			uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;

			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIElement.Append(uIPanel);

			modInfo = new UIMessageBox("This is a test of mod info here.");
			modInfo.Width.Set(-25f, 1f);
			modInfo.Height.Set(0f, 1f);
			uIPanel.Append(modInfo);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-20, 1f);
			uIScrollbar.VAlign = 0.5f;
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);

			modInfo.SetScrollbar(uIScrollbar);
			uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoHeader"), 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);

			modHomepageButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitHomepage"), 1f, false);
			modHomepageButton.Width.Set(-10f, 1f);
			modHomepageButton.Height.Set(25f, 0f);
			modHomepageButton.VAlign = 1f;
			modHomepageButton.Top.Set(-65f, 0f);
			modHomepageButton.OnMouseOver += UICommon.FadedMouseOver;
			modHomepageButton.OnMouseOut += UICommon.FadedMouseOut;
			modHomepageButton.OnClick += VisitModHomePage;
			uIElement.Append(modHomepageButton);

			UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			backButton.Width.Set(-10f, 0.333f);
			backButton.Height.Set(25f, 0f);
			backButton.VAlign = 1f;
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			extractButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtract"), 1f, false);
			extractButton.Width.Set(-10f, 0.333f);
			extractButton.Height.Set(25f, 0f);
			extractButton.VAlign = 1f;
			extractButton.HAlign = 0.5f;
			extractButton.Top.Set(-20f, 0f);
			extractButton.OnMouseOver += UICommon.FadedMouseOver;
			extractButton.OnMouseOut += UICommon.FadedMouseOut;
			extractButton.OnClick += ExtractClick;
			uIElement.Append(extractButton);

			deleteButton = new UITextPanel<string>(Language.GetTextValue("UI.Delete"), 1f, false);
			deleteButton.Width.Set(-10f, 0.333f);
			deleteButton.Height.Set(25f, 0f);
			deleteButton.VAlign = 1f;
			deleteButton.HAlign = 1f;
			deleteButton.Top.Set(-20f, 0f);
			deleteButton.OnMouseOver += UICommon.FadedMouseOver;
			deleteButton.OnMouseOut += UICommon.FadedMouseOut;
			deleteButton.OnClick += DeleteClick;
			uIElement.Append(deleteButton);

			Append(uIElement);
		}

		internal void SetModInfo(string text)
		{
			info = text;
			if (info.Equals(""))
			{
				info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
		}

		internal void SetModName(string text)
		{
			modDisplayName = text;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		internal void SetURL(string url)
		{
			this.url = url;
		}

		internal void SetMod(LocalMod mod)
		{
			localMod = mod;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11);
			Main.menuMode = gotoMenu;
		}

		private void ExtractClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			Interface.extractMod.SetMod(localMod);
			Interface.extractMod.SetGotoMenu(gotoMenu);
			Main.menuMode = Interface.extractModID;
		}

		private void DeleteClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuClose);
			File.Delete(localMod.modFile.path);
			Main.menuMode = this.gotoMenu;
		}

		private void VisitModHomePage(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10);
			Process.Start(url);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = this.gotoMenu;
		}

		public override void OnActivate()
		{
			uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + modDisplayName, 0.8f, true);
			modInfo.SetText(info);
			if (url.Equals(""))
			{
				modHomepageButton.Remove();
			}
			else
			{
				uIElement.Append(modHomepageButton);
			}
			if (localMod != null)
			{
				uIElement.Append(deleteButton);
				uIElement.Append(extractButton);
			}
			else
			{
				deleteButton.Remove();
				extractButton.Remove();
			}
		}
	}
}
