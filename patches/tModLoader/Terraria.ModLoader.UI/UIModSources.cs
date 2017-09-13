using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIModSources : UIState
	{
		private UIList modList;
		private readonly List<UIModSourceItem> items = new List<UIModSourceItem>();
		private bool updateNeeded;
		private UIElement uIElement;
		private UIPanel uIPanel;
		private UILoaderAnimatedImage uiLoader;

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

			uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

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
			UITextPanel<string> uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModSources"), 0.8f, true);
			uIHeaderTextPanel.HAlign = 0.5f;
			uIHeaderTextPanel.Top.Set(-35f, 0f);
			uIHeaderTextPanel.SetPadding(15f);
			uIHeaderTextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uIHeaderTextPanel);
			UITextPanel<string> buttonBA = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSBuildAll"), 1f, false);
			buttonBA.Width.Set(-10f, 0.5f);
			buttonBA.Height.Set(25f, 0f);
			buttonBA.VAlign = 1f;
			buttonBA.Top.Set(-65f, 0f);
			buttonBA.OnMouseOver += UICommon.FadedMouseOver;
			buttonBA.OnMouseOut += UICommon.FadedMouseOut;
			buttonBA.OnClick += BuildMods;
			uIElement.Append(buttonBA);
			UITextPanel<string> buttonBRA = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"), 1f, false);
			buttonBRA.CopyStyle(buttonBA);
			buttonBRA.HAlign = 1f;
			buttonBRA.OnMouseOver += UICommon.FadedMouseOver;
			buttonBRA.OnMouseOut += UICommon.FadedMouseOut;
			buttonBRA.OnClick += BuildAndReload;
			uIElement.Append(buttonBRA);
			UITextPanel<string> buttonB = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			buttonB.CopyStyle(buttonBA);
			buttonB.Width.Set(-10f, 1f / 3f);
			buttonB.Top.Set(-20f, 0f);
			buttonB.OnMouseOver += UICommon.FadedMouseOver;
			buttonB.OnMouseOut += UICommon.FadedMouseOut;
			buttonB.OnClick += BackClick;
			uIElement.Append(buttonB);
			UITextPanel<string> buttonOS = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSOpenSources"), 1f, false);
			buttonOS.CopyStyle(buttonB);
			buttonOS.HAlign = .5f;
			buttonOS.OnMouseOver += UICommon.FadedMouseOver;
			buttonOS.OnMouseOut += UICommon.FadedMouseOut;
			buttonOS.OnClick += OpenSources;
			uIElement.Append(buttonOS);
			UITextPanel<string> buttonMP = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSManagePublished"), 1f, false);
			buttonMP.CopyStyle(buttonB);
			buttonMP.HAlign = 1f;
			buttonMP.OnMouseOver += UICommon.FadedMouseOver;
			buttonMP.OnMouseOut += UICommon.FadedMouseOut;
			buttonMP.OnClick += ManagePublished;
			uIElement.Append(buttonMP);
			base.Append(uIElement);
		}

		private void ManagePublished(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.managePublishedID;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private void OpenSources(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModLoader.ModSourcePath);
			Process.Start(ModLoader.ModSourcePath);
		}

		private void BuildMods(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			if (modList.Count > 0)
			{
				ModLoader.reloadAfterBuild = false;
				ModLoader.buildAll = true;
				Main.menuMode = Interface.buildAllModsID;
			}
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			if (modList.Count > 0)
			{
				ModLoader.reloadAfterBuild = true;
				ModLoader.buildAll = true;
				Main.menuMode = Interface.buildAllModsID;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate()
		{
			uIPanel.Append(uiLoader);
			modList.Clear();
			items.Clear();
			Populate();
		}

		internal void Populate()
		{
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			Task.Factory
				.StartNew(delegate
				{
					var mods = ModLoader.FindModSources();
					var modFiles = ModLoader.FindMods();
					return Tuple.Create(mods, modFiles);
				})
				.ContinueWith(task =>
				{
					string[] mods = task.Result.Item1;
					TmodFile[] modFiles = task.Result.Item2;
					foreach (string mod in mods)
					{

						bool publishable = false;
						DateTime lastBuildTime = new DateTime();

						foreach (TmodFile file in modFiles)
						{
							var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.path);
							if (fileNameWithoutExtension != null && fileNameWithoutExtension.Equals(Path.GetFileName(mod)))
							{
								lastBuildTime = File.GetLastWriteTime(file.path);
								publishable = true;
								break;
							}
						}
						items.Add(new UIModSourceItem(mod, publishable, lastBuildTime));
					}
					updateNeeded = true;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			uIPanel.RemoveChild(uiLoader);
			modList.Clear();
			modList.AddRange(items);
		}
	}
}
