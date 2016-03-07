using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.UI
{
	internal class UIModSources : UIState
	{
		private UIList modList;

		public override void OnInitialize()
		{
			UIElement uIElement = new UIElement();
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
			modList = new UIList();
			modList.Width.Set(-25f, 1f);
			modList.Height.Set(0f, 1f);
			modList.ListPadding = 5f;
			uIPanel.Append(modList);
			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			modList.SetScrollbar(uIScrollbar);
			UITextPanel uITextPanel = new UITextPanel("Mod Sources", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UITextPanel button = new UITextPanel("Build All", 1f, false);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(BuildMods);
			uIElement.Append(button);
			UITextPanel button2 = new UITextPanel("Build + Reload All", 1f, false);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button2.OnClick += new UIElement.MouseEvent(BuildAndReload);
			uIElement.Append(button2);
			UITextPanel button3 = new UITextPanel("Back", 1f, false);
			button3.CopyStyle(button);
			button3.Width.Set(-10f, 1f / 3f);
			button3.Top.Set(-20f, 0f);
			button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button3.OnClick += new UIElement.MouseEvent(BackClick);
			uIElement.Append(button3);
			UITextPanel button4 = new UITextPanel("Open Sources", 1f, false);
			button4.CopyStyle(button3);
			button4.HAlign = .5f;
			button4.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button4.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button4.OnClick += new UIElement.MouseEvent(OpenSources);
			uIElement.Append(button4);
			UITextPanel button5 = new UITextPanel("Manage Published", 1f, false);
			button5.CopyStyle(button3);
			button5.HAlign = 1f;
			button5.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button5.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button5.OnClick += new UIElement.MouseEvent(ManagePublished);
			uIElement.Append(button5);
			base.Append(uIElement);
		}

		private void ManagePublished(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.managePublishedID;
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

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private static void OpenSources(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModLoader.ModSourcePath);
			Process.Start(ModLoader.ModSourcePath);
		}

		private static void BuildMods(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.reloadAfterBuild = false;
			ModLoader.buildAll = true;
			Main.menuMode = Interface.buildAllModsID;
		}

		private static void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.reloadAfterBuild = true;
			ModLoader.buildAll = true;
			Main.menuMode = Interface.buildAllModsID;
		}

		public override void OnActivate()
		{
			modList.Clear();
			string[] mods = ModLoader.FindModSources();
			TmodFile[] modFiles = ModLoader.FindMods();
			foreach (string mod in mods)
			{
				bool publishable = false;
				foreach (TmodFile file in modFiles)
				{
					if (Path.GetFileNameWithoutExtension(file.path).Equals(Path.GetFileName(mod)))
					{
						publishable = true;
						break;
					}
				}
				modList.Add(new UIModSourceItem(mod, publishable));
			}
		}
	}
}
