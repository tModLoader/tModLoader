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
			UITextPanel<string> uITextPanel = new UITextPanel<string>("Mod Sources", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UITextPanel<string> button = new UITextPanel<string>("Build All", 1f, false);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += FadedMouseOver;
			button.OnMouseOut += FadedMouseOut;
			button.OnClick += BuildMods;
			uIElement.Append(button);
			UITextPanel<string> button2 = new UITextPanel<string>("Build + Reload All", 1f, false);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += FadedMouseOver;
			button2.OnMouseOut += FadedMouseOut;
			button2.OnClick += BuildAndReload;
			uIElement.Append(button2);
			UITextPanel<string> button3 = new UITextPanel<string>("Back", 1f, false);
			button3.CopyStyle(button);
			button3.Width.Set(-10f, 1f / 3f);
			button3.Top.Set(-20f, 0f);
			button3.OnMouseOver += FadedMouseOver;
			button3.OnMouseOut += FadedMouseOut;
			button3.OnClick += BackClick;
			uIElement.Append(button3);
			UITextPanel<string> button4 = new UITextPanel<string>("Open Sources", 1f, false);
			button4.CopyStyle(button3);
			button4.HAlign = .5f;
			button4.OnMouseOver += FadedMouseOver;
			button4.OnMouseOut += FadedMouseOut;
			button4.OnClick += OpenSources;
			uIElement.Append(button4);
			UITextPanel<string> button5 = new UITextPanel<string>("Manage Published", 1f, false);
			button5.CopyStyle(button3);
			button5.HAlign = 1f;
			button5.OnMouseOver += FadedMouseOver;
			button5.OnMouseOut += FadedMouseOut;
			button5.OnClick += ManagePublished;
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
				DateTime lastBuildTime = new DateTime();
				foreach (TmodFile file in modFiles)
				{
					if (Path.GetFileNameWithoutExtension(file.path).Equals(Path.GetFileName(mod)))
					{
						lastBuildTime = File.GetLastWriteTime(file.path);
						publishable = true;
						break;
					}
				}
				modList.Add(new UIModSourceItem(mod, publishable, lastBuildTime));
			}
		}
	}
}
