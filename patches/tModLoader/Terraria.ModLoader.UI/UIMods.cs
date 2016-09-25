using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Newtonsoft.Json;

namespace Terraria.ModLoader.UI
{
	internal class UIMods : UIState
	{
		private UIList modList;
		private UIList modListAll;
		private List<UIModItem> items = new List<UIModItem>();
		private UIInputTextField filterTextBox;
		internal string filter;

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
			modListAll = new UIList();
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
			UITextPanel uITextPanel = new UITextPanel("Mods List", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UIColorTextPanel button = new UIColorTextPanel("Enable All", Color.Green, 1f, false);
			button.Width.Set(-10f, 1f / 3f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(this.EnableAll);
			uIElement.Append(button);
			UIColorTextPanel button2 = new UIColorTextPanel("Disable All", Color.Red, 1f, false);
			button2.CopyStyle(button);
			button2.HAlign = 0.5f;
			button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button2.OnClick += new UIElement.MouseEvent(this.DisableAll);
			uIElement.Append(button2);
			UITextPanel button3 = new UITextPanel("Reload Mods", 1f, false);
			button3.CopyStyle(button);
			button3.HAlign = 1f;
			button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button3.OnClick += new UIElement.MouseEvent(ReloadMods);
			uIElement.Append(button3);
			UITextPanel button4 = new UITextPanel("Back", 1f, false);
			button4.CopyStyle(button);
			button4.Top.Set(-20f, 0f);
			button4.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button4.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button4.OnClick += new UIElement.MouseEvent(BackClick);
			uIElement.Append(button4);
			UITextPanel button5 = new UITextPanel("Open Mods Folder", 1f, false);
			button5.CopyStyle(button4);
			button5.HAlign = 0.5f;
			button5.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button5.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button5.OnClick += new UIElement.MouseEvent(OpenModsFolder);
			uIElement.Append(button5);
			UIPanel panel = new UIPanel();
			panel.Top.Set(-40f, 0f);
			panel.Left.Set(-200f, 1f);
			panel.Width.Set(200f, 0f);
			panel.Height.Set(40f, 0f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIElement.Append(panel);
			filterTextBox = new UIInputTextField("Type to search");
			filterTextBox.Top.Set(-30f, 0f);
			filterTextBox.Left.Set(-180f, 1f);
			filterTextBox.OnTextChange += new UIInputTextField.EventHandler(FilterList);
			uIElement.Append(filterTextBox);
			UITextPanel modListButton = new UITextPanel("Mod Packs", 1f, false);
			modListButton.CopyStyle(button5);
			modListButton.HAlign = 1f;
			modListButton.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			modListButton.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			modListButton.OnClick += new UIElement.MouseEvent(GotoModPacksMenu);
			uIElement.Append(modListButton);
			base.Append(uIElement);
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

		private static void ReloadMods(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.Reload();
		}

		private static void OpenModsFolder(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModLoader.ModPath);
			Process.Start(ModLoader.ModPath);
		}

		private static void GotoModPacksMenu(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			Main.menuMode = Interface.modPacksMenuID;
		}

		private void EnableAll(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			foreach (UIModItem modItem in items)
			{
				if (!modItem.enabled)
				{
					modItem.ToggleEnabled(evt, listeningElement);
				}
			}
		}

		private void DisableAll(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			foreach (UIModItem modItem in items)
			{
				if (modItem.enabled)
				{
					modItem.ToggleEnabled(evt, listeningElement);
				}
			}
		}

		private void FilterList(object sender, EventArgs e)
		{
			FilterList();
		}

		private void FilterList(UIMouseEvent evt, UIElement listeningElement)
		{
			FilterList();
		}

		private void FilterList()
		{
			filter = filterTextBox.currentString;
			modList.Clear();
			foreach (UIModItem item in modListAll._items.Where(item => item.PassFilters()))
			{
				modList.Add(item);
			}
		}

		public override void OnActivate()
		{
			Main.clrInput();
			modListAll.Clear();
			items.Clear();
			TmodFile[] mods = ModLoader.FindMods();
			foreach (TmodFile mod in mods)
			{
				UIModItem modItem = new UIModItem(mod);
				modListAll.Add(modItem);
				items.Add(modItem);
			}
			FilterList();
		}
	}
}
