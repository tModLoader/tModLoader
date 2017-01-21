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
using System.Reflection;
using Terraria.GameContent.UI.States;

namespace Terraria.ModLoader.UI
{
	internal class UIModConfig : UIState
	{
		private UIList configList;
		private Mod mod;
		private ModConfig modConfig;

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

			configList = new UIList();
			configList.Width.Set(-25f, 1f);
			configList.Height.Set(0f, 1f);
			configList.ListPadding = 5f;
			uIPanel.Append(configList);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			configList.SetScrollbar(uIScrollbar);

			UITextPanel<string> uITextPanel = new UITextPanel<string>("Mod Config", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);

			UITextPanel<string> button3 = new UITextPanel<string>("Reload Mods", 1f, false);
			button3.Width.Set(-10f, 1f / 3f);
			button3.Height.Set(25f, 0f);
			button3.VAlign = 1f;
			button3.Top.Set(-65f, 0f);
			button3.HAlign = 1f;
			button3.OnMouseOver += UICommon.FadedMouseOver;
			button3.OnMouseOut += UICommon.FadedMouseOut;
			button3.OnClick += ReloadMods;
			uIElement.Append(button3);

			UITextPanel<string> button4 = new UITextPanel<string>("Back", 1f, false);
			button4.CopyStyle(button3);
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

			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;

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

		internal void SetMod(Mod mod, ModConfig modConfig)
		{
			this.mod = mod;
			this.modConfig = modConfig;
		}

		public override void OnActivate()
		{
			configList.Clear();
			int i = 0;
			// load all mod config options into UIList
			PropertyInfo[] props = this.modConfig.GetType().GetProperties();
			foreach (PropertyInfo property in props)
			{
				if (property.PropertyType == typeof(bool)) // isassignedfrom?
				{
					Console.WriteLine(property.Name + " is bool");
					//property.GetValue(this, null));

					UISortableElement uISortableElement = new UISortableElement(i++);
					uISortableElement.Width.Set(0f, 1f);
					uISortableElement.Height.Set(30f, 0f);
					uISortableElement.HAlign = 0.5f;
					configList.Add(uISortableElement);
					UIElement e = new UIModConfigBooleanItem(() => property.Name, () => (bool)property.GetValue(modConfig, null), Color.White);
					e.OnClick += (ev, v) =>
					{
						property.SetValue(modConfig, !(bool)property.GetValue(modConfig, null), null);
					};
					e.Width.Set(0f, 0.5f);
					e.HAlign = 0.5f;
					e.Height.Set(0f, 1f);
					uISortableElement.Append(e);
					//configList.Add(e);
					//(() => Lang.menu[196], () => PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap1"].Contains(Buttons.DPadUp.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap2"].Contains(Buttons.DPadRight.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap3"].Contains(Buttons.DPadDown.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap4"].Contains(Buttons.DPadLeft.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap1"].Contains(Buttons.DPadUp.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap2"].Contains(Buttons.DPadRight.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap3"].Contains(Buttons.DPadDown.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap4"].Contains(Buttons.DPadLeft.ToString()), color);
				}
				else if (property.PropertyType == typeof(float))
				{
					UISortableElement uISortableElement = new UISortableElement(i++);
					uISortableElement.Width.Set(0f, 1f);
					uISortableElement.Height.Set(30f, 0f);
					uISortableElement.HAlign = 0.5f;
					configList.Add(uISortableElement);

					UIElement e = new UIModConfigFloatItem(
						() => property.Name + ": " + (float)property.GetValue(modConfig, null),
						() => (float)property.GetValue(modConfig, null),
						(f) => property.SetValue(modConfig, f, null),
						null,
						0, Color.White);

					e.Width.Set(0f, 0.5f);
					e.HAlign = 0.5f;
					e.Height.Set(0f, 1f);
					uISortableElement.Append(e);
				}

				//object[] attrs = property.GetCustomAttributes(true);
				//foreach (object attr in attrs)
				//{
				//	ValueRangeAttribute authAttr = attr as ValueRangeAttribute;
				//	if (authAttr != null)
				//	{
				//		Console.WriteLine(authAttr.Min + " " + authAttr.Max);
				//	}
				//}
			}
		}
	}
}
