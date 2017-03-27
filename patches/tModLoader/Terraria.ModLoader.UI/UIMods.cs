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
		public bool loading;
		private UIElement uIElement;
		private UIPanel uIPanel;
		private UILoaderAnimatedImage uiLoader;
		private UIList modList;
		private UIList modListAll;
		private readonly List<UIModItem> items = new List<UIModItem>();
		private UIInputTextField filterTextBox;
		internal string filter;
		private UIColorTextPanel buttonEA;
		private UIColorTextPanel buttonDA;
		private UITextPanel<string> buttonRM;
		private UITextPanel<string> buttonB;
		private UITextPanel<string> buttonOMF;
		private UITextPanel<string> buttonMP;

		public override void OnInitialize()
		{
			uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;

			uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIElement.Append(uIPanel);

			uiLoader = new UILoaderAnimatedImage(0.5f,0.5f,1f);

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

			UITextPanel<string> uIHeaderTexTPanel = new UITextPanel<string>("Mods List", 0.8f, true);
			uIHeaderTexTPanel.HAlign = 0.5f;
			uIHeaderTexTPanel.Top.Set(-35f, 0f);
			uIHeaderTexTPanel.SetPadding(15f);
			uIHeaderTexTPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uIHeaderTexTPanel);
			buttonEA = new UIColorTextPanel("Enable All", Color.Green, 1f, false);
			buttonEA.Width.Set(-10f, 1f / 3f);
			buttonEA.Height.Set(25f, 0f);
			buttonEA.VAlign = 1f;
			buttonEA.Top.Set(-65f, 0f);
			buttonEA.OnMouseOver += UICommon.FadedMouseOver;
			buttonEA.OnMouseOut += UICommon.FadedMouseOut;
			buttonEA.OnClick += this.EnableAll;
			uIElement.Append(buttonEA);
			buttonDA = new UIColorTextPanel("Disable All", Color.Red, 1f, false);
			buttonDA.CopyStyle(buttonEA);
			buttonDA.HAlign = 0.5f;
			buttonDA.OnMouseOver += UICommon.FadedMouseOver;
			buttonDA.OnMouseOut += UICommon.FadedMouseOut;
			buttonDA.OnClick += this.DisableAll;
			uIElement.Append(buttonDA);
			buttonRM = new UITextPanel<string>("Reload Mods", 1f, false);
			buttonRM.CopyStyle(buttonEA);
			buttonRM.HAlign = 1f;
			buttonRM.OnMouseOver += UICommon.FadedMouseOver;
			buttonRM.OnMouseOut += UICommon.FadedMouseOut;
			buttonRM.OnClick += ReloadMods;
			uIElement.Append(buttonRM);
			buttonB = new UITextPanel<string>("Back", 1f, false);
			buttonB.CopyStyle(buttonEA);
			buttonB.Top.Set(-20f, 0f);
			buttonB.OnMouseOver += UICommon.FadedMouseOver;
			buttonB.OnMouseOut += UICommon.FadedMouseOut;
			buttonB.OnClick += BackClick;
			uIElement.Append(buttonB);
			buttonOMF = new UITextPanel<string>("Open Mods Folder", 1f, false);
			buttonOMF.CopyStyle(buttonB);
			buttonOMF.HAlign = 0.5f;
			buttonOMF.OnMouseOver += UICommon.FadedMouseOver;
			buttonOMF.OnMouseOut += UICommon.FadedMouseOut;
			buttonOMF.OnClick += OpenModsFolder;
			uIElement.Append(buttonOMF);
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
			buttonMP = new UITextPanel<string>("Mod Packs", 1f, false);
			buttonMP.CopyStyle(buttonOMF);
			buttonMP.HAlign = 1f;
			buttonMP.OnMouseOver += UICommon.FadedMouseOver;
			buttonMP.OnMouseOut += UICommon.FadedMouseOut;
			buttonMP.OnClick += GotoModPacksMenu;
			uIElement.Append(buttonMP);
			Append(uIElement);
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private void ReloadMods(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			if (items.Count > 0)
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
			if(uIPanel.HasChild(uiLoader)) uIPanel.RemoveChild(uiLoader);
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
			modList.Clear();
			modListAll.Clear();
			items.Clear();
			if(!uIPanel.HasChild(uiLoader)) uIPanel.Append(uiLoader);
			Populate();
		}

		internal void Populate()
		{
			loading = true;
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
					loading = false;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
