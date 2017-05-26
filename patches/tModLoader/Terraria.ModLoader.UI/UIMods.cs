using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Newtonsoft.Json;
using System.Reflection;

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
		public UICycleImage SearchFilterToggle;
		public ModsMenuSortMode sortMode = ModsMenuSortMode.RecentlyUpdated;
		public EnabledFilter enabledFilterMode = EnabledFilter.All;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();
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
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);

			uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			modListAll = new UIList();
			modList = new UIList();
			modList.Width.Set(-25f, 1f);
			modList.Height.Set(-50f, 1f);
			modList.Top.Set(50f, 0f);
			modList.ListPadding = 5f;
			uIPanel.Append(modList);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-50f, 1f);
			uIScrollbar.Top.Set(50f, 0f);
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

			Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			UIElement upperMenuContainer = new UIElement();
			upperMenuContainer.Width.Set(0f, 1f);
			upperMenuContainer.Height.Set(32f, 0f);
			upperMenuContainer.Top.Set(10f, 0f);

			UICycleImage toggleImage;
			for (int j = 0; j < 2; j++)
			{
				if (j == 0)
				{
					toggleImage = new UICycleImage(texture, 3, 32, 32, 34 * 3, 0);
					toggleImage.setCurrentState((int)sortMode);
					toggleImage.OnClick += (a, b) =>
					{
						sortMode = sortMode.NextEnum();
						FilterList();
					};
					toggleImage.OnRightClick += (a, b) =>
					{
						sortMode = sortMode.PreviousEnum();
						FilterList();
					};
				}
				else
				{
					toggleImage = new UICycleImage(texture, 3, 32, 32, 34 * 4, 0);
					toggleImage.setCurrentState((int)enabledFilterMode);
					toggleImage.OnClick += (a, b) =>
					{
						enabledFilterMode = enabledFilterMode.NextEnum();
						FilterList();
					};
					toggleImage.OnRightClick += (a, b) =>
					{
						enabledFilterMode = enabledFilterMode.PreviousEnum();
						FilterList();
					};
				}
				toggleImage.Left.Set((float)(j * 36 + 8), 0f);
				_categoryButtons.Add(toggleImage);
				upperMenuContainer.Append(toggleImage);
			}

			UIPanel filterTextBoxBackground = new UIPanel();
			filterTextBoxBackground.Top.Set(0f, 0f);
			filterTextBoxBackground.Left.Set(-170f, 1f);
			filterTextBoxBackground.Width.Set(135f, 0f);
			filterTextBoxBackground.Height.Set(40f, 0f);
			upperMenuContainer.Append(filterTextBoxBackground);

			filterTextBox = new UIInputTextField("Type to search");
			filterTextBox.Top.Set(5f, 0f);
			filterTextBox.Left.Set(-160f, 1f);
			filterTextBox.OnTextChange += new UIInputTextField.EventHandler(FilterList);
			upperMenuContainer.Append(filterTextBox);

			SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 34 * 2, 0);
			SearchFilterToggle.setCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) =>
			{
				searchFilterMode = searchFilterMode.NextEnum();
				FilterList();
			};
			SearchFilterToggle.OnRightClick += (a, b) =>
			{
				searchFilterMode = searchFilterMode.PreviousEnum();
				FilterList();
			};
			SearchFilterToggle.Left.Set(545f, 0f);
			_categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);

			buttonMP = new UITextPanel<string>("Mod Packs", 1f, false);
			buttonMP.CopyStyle(buttonOMF);
			buttonMP.HAlign = 1f;
			buttonMP.OnMouseOver += UICommon.FadedMouseOver;
			buttonMP.OnMouseOut += UICommon.FadedMouseOut;
			buttonMP.OnClick += GotoModPacksMenu;
			uIElement.Append(buttonMP);

			uIPanel.Append(upperMenuContainer);
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
			if (uIPanel.HasChild(uiLoader)) uIPanel.RemoveChild(uiLoader);
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
			for (int i = 0; i < this._categoryButtons.Count; i++)
			{
				if (this._categoryButtons[i].IsMouseHovering)
				{
					string text;
					switch (i)
					{
						case 0:
							text = sortMode.ToFriendlyString();
							break;
						case 1:
							text = enabledFilterMode.ToFriendlyString();
							break;
						case 2:
							text = searchFilterMode.ToFriendlyString();
							break;
						default:
							text = "None";
							break;
					}
					float x = Main.fontMouseText.MeasureString(text).X;
					Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
					if (vector.Y > (float)(Main.screenHeight - 30))
					{
						vector.Y = (float)(Main.screenHeight - 30);
					}
					if (vector.X > (float)Main.screenWidth - x)
					{
						vector.X = (float)(Main.screenWidth - x - 30);
					}
					Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
					return;
				}
			}
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate()
		{
			Main.clrInput();
			modList.Clear();
			modListAll.Clear();
			items.Clear();
			if (!uIPanel.HasChild(uiLoader)) uIPanel.Append(uiLoader);
			Populate();
		}

		internal void Populate()
		{
			loading = true;
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
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

	public static class ModsMenuSortModesExtensions
	{
		public static string ToFriendlyString(this ModsMenuSortMode sortmode)
		{
			switch (sortmode)
			{
				case ModsMenuSortMode.RecentlyUpdated:
					return "Sort by recently changed";
				case ModsMenuSortMode.DisplayNameAtoZ:
					return "Sort mod names alphabetically";
				case ModsMenuSortMode.DisplayNameZtoA:
					return "Sort mod names reverse-alphabetically";
			}
			return "Unknown Sort";
		}
	}

	public static class EnabledFilterModesExtensions
	{
		public static string ToFriendlyString(this EnabledFilter updateFilterMode)
		{
			switch (updateFilterMode)
			{
				case EnabledFilter.All:
					return "Show all mods";
				case EnabledFilter.EnabledOnly:
					return "Show only enabled mods";
				case EnabledFilter.DisabledOnly:
					return "Show only disabled mods";
			}
			return "Unknown Sort";
		}
	}

	public enum ModsMenuSortMode
	{
		RecentlyUpdated,
		DisplayNameAtoZ,
		DisplayNameZtoA,
	}

	public enum EnabledFilter
	{
		All,
		EnabledOnly,
		DisabledOnly,
	}
}
