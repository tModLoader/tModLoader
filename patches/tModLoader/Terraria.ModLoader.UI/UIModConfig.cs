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
	// TODO: In-game version of this UI.
	internal class UIModConfig : UIState
	{
		private UIElement uIElement;
		private UITextPanel<string> headerTextPanel;
		private UITextPanel<string> previousConfigButton;
		private UITextPanel<string> nextConfigButton;
		private UITextPanel<string> saveConfigButton;
		private UITextPanel<string> revertConfigButton;
		private UIList configList;
		private Mod mod;
		private List<ModConfig> modConfigs;
		// This is from ConfigManager.Configs
		private ModConfig modConfig;
		// the clone we modify. 
		private ModConfig modConfigClone;

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

			headerTextPanel = new UITextPanel<string>("Mod Config", 0.8f, true);
			headerTextPanel.HAlign = 0.5f;
			headerTextPanel.Top.Set(-50f, 0f);
			headerTextPanel.SetPadding(15f);
			headerTextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(headerTextPanel);

			previousConfigButton = new UITextPanel<string>("Previous Config", 1f, false);
			previousConfigButton.Width.Set(-10f, 1f / 3f);
			previousConfigButton.Height.Set(25f, 0f);
			previousConfigButton.VAlign = 1f;
			previousConfigButton.Top.Set(-65f, 0f);
			previousConfigButton.HAlign = 0f;
			previousConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			previousConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			previousConfigButton.OnClick += PreviousConfig;
			//uIElement.Append(previousConfigButton);

			nextConfigButton = new UITextPanel<string>("Next Config", 1f, false);
			nextConfigButton.CopyStyle(previousConfigButton);
			nextConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			nextConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			nextConfigButton.HAlign = 1f;
			nextConfigButton.OnClick += NextConfig;
			//uIElement.Append(nextConfigButton);

			saveConfigButton = new UITextPanel<string>("Save Config", 1f, false);
			saveConfigButton.CopyStyle(previousConfigButton);
			saveConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			saveConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			saveConfigButton.HAlign = 0.5f;
			saveConfigButton.OnClick += SaveConfig;
			//uIElement.Append(saveConfigButton);

			UITextPanel<string> backButton = new UITextPanel<string>("Back", 1f, false);
			backButton.CopyStyle(previousConfigButton);
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			revertConfigButton = new UITextPanel<string>("Revert Changes", 1f, false);
			revertConfigButton.CopyStyle(previousConfigButton);
			revertConfigButton.Top.Set(-20f, 0f);
			revertConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			revertConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			revertConfigButton.HAlign = 0.5f;
			revertConfigButton.OnClick += RevertConfig;
			//uIElement.Append(revertConfigButton);

			//UITextPanel<string> button5 = new UITextPanel<string>("Open Mods Folder", 1f, false);
			//button5.CopyStyle(backButton);
			//button5.HAlign = 0.5f;
			//button5.OnMouseOver += UICommon.FadedMouseOver;
			//button5.OnMouseOut += UICommon.FadedMouseOut;
			//button5.OnClick += OpenModsFolder;
			//uIElement.Append(button5);

			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;

			Append(uIElement);
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuClose);
			Main.menuMode = Interface.modsMenuID;
			if (ConfigManager.ModNeedsReload(mod))
			{
				Main.menuMode = Interface.reloadModsID;
			}
		}

		// TODO: with in-game version, disable MultiplayerSyncMode.ServerDictates configs (View Only maybe?)
		private void PreviousConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			//DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index - 1 < 0 ? modConfigs.Count - 1 : index - 1];
			//modConfigClone = modConfig.Clone();
			Main.menuMode = Interface.modConfigID;
		}

		private void NextConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index + 1 > modConfigs.Count ? 0 : index + 1];
			//modConfigClone = modConfig.Clone();
			Main.menuMode = Interface.modConfigID;
		}

		// TODO: With in-game version prevent save that would cause ReloadRequired to be true?
		// Applies the changes to 
		private void SaveConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			// save takes clone and saves to disk, but how can we know if we need to reload
			ConfigManager.Save(modConfigClone);
			ConfigManager.Load(modConfig);

			//if (ConfigManager.ModNeedsReload(modConfig.mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
			//else
			//{
			Main.menuMode = Interface.modConfigID;
			//}

			// RELOAD HERE!
		}

		private void RevertConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			DiscardChanges();
		}

		private void DiscardChanges()
		{
			// reclone, then reload this UI
			//modConfigClone = modConfig.Clone();
			Main.menuMode = Interface.modConfigID;
		}

		public void SetPendingChanges(bool changes = true)
		{
			if (changes)
			{
				uIElement.Append(saveConfigButton);
				uIElement.Append(revertConfigButton);
			}
			else
			{
				uIElement.RemoveChild(saveConfigButton);
				uIElement.RemoveChild(revertConfigButton);
			}
		}

		// do we need 2 copies? We can discard changes by reloading.
		// We can save pending changes by saving file then loading/reloading mods.
		// when we get new server configs from server...replace, don't save?
		// reload manually, reload fresh server config?
		// need some CopyTo method to preserve references....hmmm
		internal void SetMod(Mod mod)
		{
			this.mod = mod;
			if (ConfigManager.Configs.ContainsKey(mod))
			{
				modConfigs = ConfigManager.Configs[mod];
				modConfig = modConfigs[0];
				//modConfigClone = modConfig.Clone();
			}
			else
			{
				throw new Exception($"There are no ModConfig for {mod.DisplayName}, how did this happen?");
			}
		}

		public override void OnActivate()
		{
			headerTextPanel.SetText(modConfig.mod.DisplayName + ": " + modConfig.Name);
			modConfigClone = modConfig.Clone();
			int index = modConfigs.IndexOf(modConfig);
			int count = modConfigs.Count;
			uIElement.RemoveChild(saveConfigButton);
			uIElement.RemoveChild(revertConfigButton);
			uIElement.RemoveChild(previousConfigButton);
			uIElement.RemoveChild(nextConfigButton);
			if (index + 1 < count)
				uIElement.Append(nextConfigButton);
			if (index - 1 >= 0)
				uIElement.Append(previousConfigButton);

			configList.Clear();
			int i = 0;
			// load all mod config options into UIList
			PropertyInfo[] props = modConfigClone.GetType().GetProperties(
				BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			FieldInfo[] fields = modConfigClone.GetType().GetFields(
				BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			//const BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
			//MemberInfo[] members = modConfig.GetType().GetFields(bindingFlags).Cast<MemberInfo>()
			//	.Concat(modConfig.GetType().GetProperties(bindingFlags)).ToArray();

			foreach (FieldInfo field in fields)
			{
				UISortableElement uISortableElement = new UISortableElement(i++);
				uISortableElement.Width.Set(0f, 1f);
				uISortableElement.Height.Set(30f, 0f);
				uISortableElement.HAlign = 0.5f;
				configList.Add(uISortableElement);

				if (field.FieldType == typeof(bool)) // isassignedfrom?
				{
					UIElement e = new UIModConfigBooleanItem(field, modConfigClone);
					uISortableElement.Append(e);
				}
				else
				{
					uISortableElement.Append(new UIText($"{field.Name} not handled yet ({field.FieldType.Name})"));
				}
			}

			foreach (PropertyInfo property in props)
			{
				if (property.Name == "Mode")
					continue;
				UISortableElement uISortableElement = new UISortableElement(i++);
				uISortableElement.Width.Set(0f, 1f);
				uISortableElement.Height.Set(30f, 0f);
				uISortableElement.HAlign = 0.5f;
				configList.Add(uISortableElement);

				if (property.PropertyType == typeof(bool)) // isassignedfrom?
				{

					LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(property, typeof(LabelAttribute));

					if (att != null)
					{
						Console.WriteLine(att);
					}

					UIElement e = new UIModConfigBooleanItem(property, modConfigClone);

					//UIElement e = new UIModConfigBooleanItem(
					//	() => property.Name,
					//	() => (bool)property.GetValue(modConfig, null),
					//	Color.White);
					//e.OnClick += (ev, v) =>
					//{
					//	property.SetValue(modConfig, !(bool)property.GetValue(modConfig, null), null);
					//};

					//e.Width.Set(0f, 0.5f);
					//e.HAlign = 0.5f;
					//e.Height.Set(0f, 1f);
					uISortableElement.Append(e);
					//configList.Add(e);
					//(() => Lang.menu[196], () => PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap1"].Contains(Buttons.DPadUp.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap2"].Contains(Buttons.DPadRight.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap3"].Contains(Buttons.DPadDown.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad].KeyStatus["DpadSnap4"].Contains(Buttons.DPadLeft.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap1"].Contains(Buttons.DPadUp.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap2"].Contains(Buttons.DPadRight.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap3"].Contains(Buttons.DPadDown.ToString()) && PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepadUI].KeyStatus["DpadSnap4"].Contains(Buttons.DPadLeft.ToString()), color);
				}
				else if (property.PropertyType == typeof(float))
				{
					UIElement e = new UIModConfigFloatItem(
						() => property.Name + ": " + (float)property.GetValue(modConfigClone, null),
						() => (float)property.GetValue(modConfigClone, null),
						(f) => property.SetValue(modConfigClone, f, null),
						null,
						0, Color.White);

					//e.Width.Set(0f, 0.5f);
					//e.HAlign = 0.5f;
					//e.Height.Set(0f, 1f);
					uISortableElement.Append(e);
				}
				else
				{
					uISortableElement.Append(new UIText($"{property.Name} not handled yet ({property.PropertyType.Name})"));
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
