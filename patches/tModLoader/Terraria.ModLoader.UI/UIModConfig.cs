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
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

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

		bool pendingChanges;
		public void SetPendingChanges(bool changes = true)
		{
			pendingChanges |= changes;
		}

		public override void Update(GameTime gameTime)
		{
			if (pendingChanges)
			{
				uIElement.Append(saveConfigButton);
				uIElement.Append(revertConfigButton);
				pendingChanges = false;
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
			// TODO: Inheritance with ModConfig? DeclaredOnly?
			PropertyInfo[] properties = modConfigClone.GetType().GetProperties(
				BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			FieldInfo[] fields = modConfigClone.GetType().GetFields(
				BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			var fieldsAndProperties = fields.Select(x => new PropertyFieldWrapper(x)).Concat(properties.Select(x => new PropertyFieldWrapper(x)));

			foreach (PropertyFieldWrapper variable in fieldsAndProperties)
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;

				UISortableElement sortedContainer = new UISortableElement(i++);
				sortedContainer.Width.Set(0f, 1f);
				sortedContainer.Height.Set(30f, 0f);
				sortedContainer.HAlign = 0.5f;
				configList.Add(sortedContainer);

				Type type = variable.Type;
				if (type == typeof(bool)) // isassignedfrom?
				{
					UIElement e = new UIModConfigBooleanItem(variable, modConfigClone);
					sortedContainer.Append(e);
				}
				else if (type == typeof(float))
				{
					UIElement e = new UIModConfigFloatItem(variable, modConfigClone, i);

					//UIElement e = new UIModConfigFloatItem(
					//	() => variable.Name + ": " + (float)variable.GetValue(modConfigClone),
					//	() => (float)variable.GetValue(modConfigClone),
					//	(f) => variable.SetValue(modConfigClone, f),
					//	null,
					//	0, Color.White);

					sortedContainer.Append(e);
				}
				else if (type == typeof(int))
				{
					UIElement e = new UIModConfigIntItem(variable, modConfigClone, i);
					sortedContainer.Append(e);
				}
				else if (type == typeof(string))
				{
					OptionStringsAttribute att = (OptionStringsAttribute)Attribute.GetCustomAttribute(variable.MemberInfo, typeof(OptionStringsAttribute));
					if (att != null)
					{
						UIElement e = new UIModConfigStringItem(variable, modConfigClone, i);
						sortedContainer.Append(e);
					}
					else
					{
						// TODO: Text input? Necessary?
						sortedContainer.Append(new UIText($"{variable.Name} not handled yet ({type.Name}). Missing OptionStringsAttribute."));
					}
				}
				else if (type.IsEnum)
				{
					UIElement e = new UIModConfigEnumItem(variable, modConfigClone, i);
					sortedContainer.Append(e);
				}
				else if (type.IsArray)
				{
					sortedContainer.Append(new UIText($"{variable.Name} not handled yet ({type.Name})"));
				}
				else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				{
					UIElement e = new UIModConfigListItem(variable, modConfigClone, ref i);
					sortedContainer.Height.Pixels = 225;
					sortedContainer.Append(e);
				}
				else
				{
					sortedContainer.Append(new UIText($"{variable.Name} not handled yet ({type.Name})"));
				}
			}
		}
	}

	internal class PropertyFieldWrapper
	{
		private FieldInfo fieldInfo;
		private PropertyInfo propertyInfo;

		public PropertyFieldWrapper(FieldInfo fieldInfo)
		{
			this.fieldInfo = fieldInfo;
		}

		public PropertyFieldWrapper(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		public bool isField => fieldInfo != null;
		public bool isProperty => propertyInfo != null;

		public MemberInfo MemberInfo => fieldInfo != null ? fieldInfo : (MemberInfo)propertyInfo;

		public string Name => fieldInfo?.Name ?? propertyInfo.Name;

		public Type Type => fieldInfo?.FieldType ?? propertyInfo.PropertyType;

		public object GetValue(Object obj)
		{
			if (fieldInfo != null)
				return fieldInfo.GetValue(obj);
			else
				return propertyInfo.GetValue(obj, null);
		}

		public void SetValue(Object obj, object value)
		{
			if (fieldInfo != null)
				fieldInfo.SetValue(obj, value);
			else
				propertyInfo.SetValue(obj, value, null);
		}
	}

	abstract class UIConfigItem : UIElement
	{
		internal bool drawTicks;
		public abstract int NumberTicks { get; }
		public abstract float TickIncrement { get; }


		public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null)
		{
			if (colorMethod == null)
			{
				colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);
			}
			Texture2D colorBarTexture = Main.colorBarTexture;
			Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height) * scale;
			IngameOptions.valuePosition.X = IngameOptions.valuePosition.X - (float)((int)vector.X);
			Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
			Rectangle destinationRectangle = rectangle;
			int num = 167;
			float num2 = (float)rectangle.X + 5f * scale;
			float num3 = (float)rectangle.Y + 4f * scale;
			if (drawTicks)
			{
				int numTicks = NumberTicks;
				if (numTicks > 1)
				{
					for (int tick = 0; tick < numTicks; tick++)
					{
						float percent = tick * TickIncrement;
						sb.Draw(Main.magicPixel, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.Blue);
					}
				}
			}
			sb.Draw(colorBarTexture, rectangle, Color.White);
			for (float num4 = 0f; num4 < (float)num; num4 += 1f)
			{
				float percent = num4 / (float)num;
				sb.Draw(Main.colorBlipTexture, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
			rectangle.Inflate((int)(-5f * scale), 2);
			//rectangle.X = (int)num2;
			//rectangle.Y = (int)num3;
			bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));
			if (lockState == 2)
			{
				flag = false;
			}
			if (flag || lockState == 1)
			{
				sb.Draw(Main.colorHighlightTexture, destinationRectangle, Main.OurFavoriteColor);
			}
			sb.Draw(Main.colorSliderTexture, new Vector2(num2 + 167f * scale * perc, num3 + 4f * scale), null, Color.White, 0f, new Vector2(0.5f * (float)Main.colorSliderTexture.Width, 0.5f * (float)Main.colorSliderTexture.Height), scale, SpriteEffects.None, 0f);
			if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width)
			{
				IngameOptions.inBar = flag;
				return (float)(Main.mouseX - rectangle.X) / (float)rectangle.Width;
			}
			IngameOptions.inBar = false;
			if (rectangle.X >= Main.mouseX)
			{
				return 0f;
			}
			return 1f;
		}
	}
}
