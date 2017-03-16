using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Gamepad;
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
			UITextPanel<string> uITextPanel = new UITextPanel<string>("Mods List", 0.8f, true);
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
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += this.EnableAll;
			uIElement.Append(button);
			UIColorTextPanel button2 = new UIColorTextPanel("Disable All", Color.Red, 1f, false);
			button2.CopyStyle(button);
			button2.HAlign = 0.5f;
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += this.DisableAll;
			uIElement.Append(button2);
			UITextPanel<string> button3 = new UITextPanel<string>("Reload Mods", 1f, false);
			button3.CopyStyle(button);
			button3.HAlign = 1f;
			button3.OnMouseOver += UICommon.FadedMouseOver;
			button3.OnMouseOut += UICommon.FadedMouseOut;
			button3.OnClick += ReloadMods;
			uIElement.Append(button3);
			UITextPanel<string> button4 = new UITextPanel<string>("Back", 1f, false);
			button4.CopyStyle(button);
			button4.Top.Set(-20f, 0f);
			button4.OnMouseOver += UICommon.FadedMouseOver;
			button4.OnMouseOut += UICommon.FadedMouseOut;
			button4.OnClick += BackClick;
			uIElement.Append(button4);
			UITextPanel<string> button5 = new UITextPanel<string>("Open Mods Folder", 1f, false);
			button5.CopyStyle(button4);
			button5.HAlign = 0.5f;
			button5.OnMouseOver += UICommon.FadedMouseOver;
			button5.OnMouseOut += UICommon.FadedMouseOut;
			button5.OnClick += OpenModsFolder;
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
			UITextPanel<string> modListButton = new UITextPanel<string>("Mod Packs", 1f, false);
			modListButton.CopyStyle(button5);
			modListButton.HAlign = 1f;
			modListButton.OnMouseOver += UICommon.FadedMouseOver;
			modListButton.OnMouseOut += UICommon.FadedMouseOut;
			modListButton.OnClick += GotoModPacksMenu;
			uIElement.Append(modListButton);
			base.Append(uIElement);
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

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate()
		{
			Main.clrInput();
			modListAll.Clear();
			items.Clear();

			Task.Factory
				.StartNew(ModLoader.FindMods)
				.ContinueWith(task =>
				{
					var mods = task.Result;
					foreach (TmodFile mod in mods)
					{
						UIModItem modItem = new UIModItem(mod);
						modListAll.Add(modItem);
						items.Add(modItem);
					}
					FilterList();
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
