using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class UIModConfigList : UIState
	{
		private UIElement uIElement;
		private UIPanel uIPanel;
		//private bool needToRemoveLoading;
		private UIList modList;
		//private readonly List<UIModItem> items = new List<UIModItem>();
		//private bool updateNeeded;
		//internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		private UITextPanel<string> buttonB;
		//private UITextPanel<string> buttonOMF;

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
			uIPanel.BackgroundColor = UICommon.MainPanelBackground;
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);

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

			UITextPanel<string> uIHeaderTexTPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModsModsList"), 0.8f, true);
			uIHeaderTexTPanel.HAlign = 0.5f;
			uIHeaderTexTPanel.Top.Set(-35f, 0f);
			uIHeaderTexTPanel.SetPadding(15f);
			uIHeaderTexTPanel.BackgroundColor = UICommon.DefaultUIBlue;
			uIElement.Append(uIHeaderTexTPanel);

			buttonB = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			buttonB.Width.Set(-10f, 1f / 3f);
			buttonB.Height.Set(25f, 0f);
			buttonB.VAlign = 1f;
			buttonB.Top.Set(-20f, 0f);
			buttonB.WithFadedMouseOver();
			buttonB.OnClick += BackClick;
			uIElement.Append(buttonB);

			//buttonOMF = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModsOpenModsFolder"), 1f, false);
			//buttonOMF.CopyStyle(buttonB);
			//buttonOMF.HAlign = 0.5f;
			//buttonOMF.OnMouseOver += UICommon.FadedMouseOver;
			//buttonOMF.OnMouseOut += UICommon.FadedMouseOut;
			//buttonOMF.OnClick += OpenModsFolder;
			//uIElement.Append(buttonOMF);

			Append(uIElement);
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			//Main.menuMode = 0;

			//Main.menuMode = 1127;
			IngameFancyUI.Close();
		}

		//private static void OpenModsFolder(UIMouseEvent evt, UIElement listeningElement)
		//{
		//	Main.PlaySound(10, -1, -1, 1);
		//	Directory.CreateDirectory(ModLoader.ModPath);
		//	Process.Start(ModLoader.ModPath);
		//}

		//public override void Draw(SpriteBatch spriteBatch)
		//{
		//	base.Draw(spriteBatch);
		//	UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		//}

		internal void Unload() {
			modList?.Clear();
		}

		public override void OnActivate()
		{
			Main.clrInput();
			modList.Clear();
			//	items.Clear();
			Populate();
		}

		internal void Populate()
		{
			int i = 0;
			foreach (var item in ConfigManager.Configs)
			{
				foreach (var config in item.Value)
				{
					//if (config.Mode == ConfigScope.ClientSide)
					{
						string configDisplayName = ((LabelAttribute)Attribute.GetCustomAttribute(config.GetType(), typeof(LabelAttribute)))?.Label ?? config.Name;
						UITextPanel<string> t = new UITextPanel<string>(item.Key.DisplayName + ": " + configDisplayName);
						//UIText t = new UIText(item.Key.DisplayName + ": " + item.Value.Count);
						t.OnClick += (a, b) =>
						{
							Interface.modConfig.SetMod(item.Key, config);
							Main.InGameUI.SetState(Interface.modConfig);
						};
						t.WithFadedMouseOver();
						t.HAlign = 0.5f;
						UIElement container = new UISortableElement(i++);
						container.Width.Set(0f, 1f);
						container.Height.Set(40f, 0f);
						container.HAlign = 1f;
						container.Append(t);
						modList.Add(container);

						if (config.Mode == ConfigScope.ServerSide)
						{
							t.BackgroundColor = Color.Pink * 0.7f;
							t.OnMouseOver += (a, b) => t.BackgroundColor = Color.Pink;
							t.OnMouseOut += (a, b) => t.BackgroundColor = Color.Pink * 0.7f;
						}
					}
				}
			}
		}
	}
}