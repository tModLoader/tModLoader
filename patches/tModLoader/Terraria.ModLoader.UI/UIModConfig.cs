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
using Terraria.GameInput;

namespace Terraria.ModLoader.UI
{
	// TODO: In-game version of this UI.
	// TODO: Dictionary
	// TODO: Item/NPC UIElements w/special ui.	
	// TODO: Revert individual button.	
	// TODO: Initialize List if null.
	// TODO: DefaultValue for new elements in Lists
	internal class UIModConfig : UIState
	{
		private UIElement uIElement;
		private UITextPanel<string> headerTextPanel;
		private UITextPanel<string> message;
		private UITextPanel<string> previousConfigButton;
		private UITextPanel<string> nextConfigButton;
		private UITextPanel<string> saveConfigButton;
		private UITextPanel<string> revertConfigButton;
		private UITextPanel<string> restoreDefaultsConfigButton;
		private UIPanel uIPanel;
		private UIList mainConfigList;
		private UIScrollbar uIScrollbar;
		private Stack<UIPanel> configPanelStack = new Stack<UIPanel>();
		private Stack<string> subPageStack = new Stack<string>();
		//private UIList currentConfigList;
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
			uIElement.Top.Set(160f, 0f);
			uIElement.Height.Set(-180f, 1f);
			uIElement.HAlign = 0.5f;

			uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIElement.Append(uIPanel);

			message = new UITextPanel<string>("Notification: ");
			message.Width.Set(-80f, 1f);
			message.Height.Set(20f, 0f);
			message.HAlign = 0.5f;
			message.VAlign = 1f;
			message.Top.Set(-65f, 0f);
			uIElement.Append(message);

			mainConfigList = new UIList();
			mainConfigList.Width.Set(-25f, 1f);
			mainConfigList.Height.Set(0f, 1f);
			//mainConfigList.Top.Set(40f, 0f);
			mainConfigList.ListPadding = 5f;
			uIPanel.Append(mainConfigList);
			configPanelStack.Push(uIPanel);
			//currentConfigList = mainConfigList;

			uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			mainConfigList.SetScrollbar(uIScrollbar);

			headerTextPanel = new UITextPanel<string>("Mod Config", 0.8f, true);
			headerTextPanel.HAlign = 0.5f;
			headerTextPanel.Top.Set(-50f, 0f);
			headerTextPanel.SetPadding(15f);
			headerTextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(headerTextPanel);

			previousConfigButton = new UITextPanel<string>("<", 1f, false);
			previousConfigButton.Width.Set(25f, 0);
			previousConfigButton.Height.Set(25f, 0f);
			previousConfigButton.VAlign = 1f;
			previousConfigButton.Top.Set(-65f, 0f);
			previousConfigButton.HAlign = 0f;
			previousConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			previousConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			previousConfigButton.OnClick += PreviousConfig;
			//uIElement.Append(previousConfigButton);

			nextConfigButton = new UITextPanel<string>(">", 1f, false);
			nextConfigButton.CopyStyle(previousConfigButton);
			nextConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			nextConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			nextConfigButton.HAlign = 1f;
			nextConfigButton.OnClick += NextConfig;
			//uIElement.Append(nextConfigButton);

			saveConfigButton = new UITextPanel<string>("Save Config", 1f, false);
			saveConfigButton.Width.Set(-10f, 1f / 4f);
			saveConfigButton.Height.Set(25f, 0f);
			saveConfigButton.Top.Set(-20f, 0f);
			saveConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			saveConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			saveConfigButton.HAlign = 0.33f;
			saveConfigButton.VAlign = 1f;
			saveConfigButton.OnClick += SaveConfig;
			//uIElement.Append(saveConfigButton);

			UITextPanel<string> backButton = new UITextPanel<string>("Back", 1f, false);
			backButton.CopyStyle(saveConfigButton);
			backButton.HAlign = 0;
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			revertConfigButton = new UITextPanel<string>("Revert Changes", 1f, false);
			revertConfigButton.CopyStyle(saveConfigButton);
			revertConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			revertConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			revertConfigButton.HAlign = 0.66f;
			revertConfigButton.OnClick += RevertConfig;
			//uIElement.Append(revertConfigButton);

			//float scale = Math.Min(1f, 130f/Main.fontMouseText.MeasureString("Restore Defaults").X);
			restoreDefaultsConfigButton = new UITextPanel<string>("Restore Defaults", 1f, false);
			restoreDefaultsConfigButton.CopyStyle(saveConfigButton);
			restoreDefaultsConfigButton.OnMouseOver += UICommon.FadedMouseOver;
			restoreDefaultsConfigButton.OnMouseOut += UICommon.FadedMouseOut;
			restoreDefaultsConfigButton.HAlign = 1f;
			restoreDefaultsConfigButton.OnClick += RestoreDefaults;
			uIElement.Append(restoreDefaultsConfigButton);

			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;

			Append(uIElement);
		}


		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuClose);
			Main.menuMode = Interface.modsMenuID;

			//Main.menuMode = 1127;
			IngameFancyUI.Close();
			//if (ConfigManager.ModNeedsReload(mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
		}

		// TODO: with in-game version, disable MultiplayerSyncMode.ServerDictates configs (View Only maybe?)
		private void PreviousConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			//DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index - 1 < 0 ? modConfigs.Count - 1 : index - 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		private void NextConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index + 1 > modConfigs.Count ? 0 : index + 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		private void DoMenuModeState()
		{
			if (Main.gameMenu)
				Main.menuMode = Interface.modConfigID;
			else
				Main.InGameUI.SetState(Interface.modConfig);
		}

		// TODO: With in-game version prevent save that would cause ReloadRequired to be true?
		// Applies the changes to 
		private void SaveConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			if (Main.gameMenu)
			{
				Main.PlaySound(ID.SoundID.MenuOpen);
				// save takes clone and saves to disk, but how can we know if we need to reload
				ConfigManager.Save(modConfigClone);
				ConfigManager.Load(modConfig); // Changes not taken effect?
											   // Reload will be forced by Back Button in UIMods if needed
			}
			else
			{
				// TODO: Server request.
				if (modConfigClone.Mode == MultiplayerSyncMode.ServerDictates && Main.netMode == ID.NetmodeID.MultiplayerClient)
				{
					SetMessage("Asking server to accept changes...", Color.Yellow);

					var requestChanges = new ModPacket(ID.MessageID.InGameChangeConfig);
					requestChanges.Write(modConfigClone.mod.Name);
					requestChanges.Write(modConfigClone.Name);
					string json = JsonConvert.SerializeObject(modConfigClone, ConfigManager.serializerSettings);
					requestChanges.Write(json);
					requestChanges.Send();

					//IngameFancyUI.Close();

					// Make a packet with just this config and send to server.
					// Server responds. On receive, Save and Load? No. Swap out pendingConfig<NetConfig> and PopulateObject
					//                      -- if UI is open, update message and reload: DoMenuModeState();

					return;
				}
				if (modConfigClone.NeedsReload(modConfig))
				{
					Main.PlaySound(ID.SoundID.MenuClose);
					SetMessage("Can't save because changes would require a reload.", Color.Red);
					return;
				}
				else
				{
					Main.PlaySound(ID.SoundID.MenuOpen);
					ConfigManager.Save(modConfigClone);
					ConfigManager.Load(modConfig);
				}
			}


			//if (ConfigManager.ModNeedsReload(modConfig.mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
			//else
			//{
			DoMenuModeState();
			//}

			// RELOAD HERE!
		}

		private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			//ConfigManager.Reset(modConfigClone);
			//ConfigManager.Save(modConfigClone);
			//ConfigManager.Load(modConfig);
			pendingRevertDefaults = true;
			DoMenuModeState();
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
			DoMenuModeState();
		}

		bool pendingChanges;
		public void SetPendingChanges(bool changes = true)
		{
			pendingChanges |= changes;
		}

		public void SetMessage(string text, Color color)
		{
			message.TextScale = 1f;
			message.SetText("Notification: " + text);
			float width = Main.fontMouseText.MeasureString(text).X;
			if (width > 400)
			{
				message.TextScale = 400 / width;
				message.Recalculate();
			}
			message.TextColor = color;
		}

		bool netUpdate;
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (pendingChanges)
			{
				uIElement.Append(saveConfigButton);
				uIElement.Append(revertConfigButton);
				pendingChanges = false;
			}
			if (netUpdate)
			{
				DoMenuModeState();
				netUpdate = false;
			}
		}

		// do we need 2 copies? We can discard changes by reloading.
		// We can save pending changes by saving file then loading/reloading mods.
		// when we get new server configs from server...replace, don't save?
		// reload manually, reload fresh server config?
		// need some CopyTo method to preserve references....hmmm
		internal void SetMod(Mod mod, ModConfig config = null)
		{
			this.mod = mod;
			if (ConfigManager.Configs.ContainsKey(mod))
			{
				modConfigs = ConfigManager.Configs[mod];
				modConfig = modConfigs[0];
				if (config != null)
				{
					modConfig = ConfigManager.Configs[mod].First(x => x == config);
					// TODO, decide which configs to show in game: modConfigs = ConfigManager.Configs[mod].Where(x => x.Mode == MultiplayerSyncMode.UniquePerPlayer).ToList();
				}
				//modConfigClone = modConfig.Clone();

				// if in game, maybe have all configs open

			}
			else
			{
				throw new Exception($"There are no ModConfig for {mod.DisplayName}, how did this happen?");
			}
		}

		static bool pendingRevertDefaults;
		public override void OnActivate()
		{
			SetMessage("", Color.White);
			headerTextPanel.SetText(modConfig.mod.DisplayName + ": " + modConfig.Name);
			modConfigClone = modConfig.Clone();
			if (pendingRevertDefaults)
			{
				pendingRevertDefaults = false;
				ConfigManager.Reset(modConfigClone);
				pendingChanges = true;
			}

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

			uIElement.RemoveChild(configPanelStack.Peek());
			uIElement.Append(uIPanel);
			mainConfigList.Clear();
			configPanelStack.Clear();
			configPanelStack.Push(uIPanel);
			subPageStack.Clear();
			//currentConfigList = mainConfigList;
			int i = 0;
			int top = 0;
			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(modConfigClone))
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
					continue;

				WrapIt(mainConfigList, ref top, variable, modConfigClone, ref i);
			}
		}

		public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, ref int sliderIDInPage, object array = null, Type arrayType = null, int index = -1)
		{
			int elementHeight = 30;
			Type type = memberInfo.Type;
			if (arrayType != null)
			{
				type = arrayType;
			}
			int original = sliderIDInPage;
			UIElement e = null;

			// TODO: Modder supplied UIElement
			// TODO: Vector2, other structs
			if (type == typeof(Color))
			{
				e = new UIModConfigColorItem(memberInfo, item, ref sliderIDInPage, (IList<Color>)array, index);
				elementHeight = (int)(e as UIModConfigColorItem).GetHeight();
			}
			else if (type == typeof(bool)) // isassignedfrom?
			{
				e = new UIModConfigBooleanItem(memberInfo, item, (IList<bool>)array, index);
				sliderIDInPage++;
			}
			else if (type == typeof(float))
			{
				e = new UIModConfigFloatItem(memberInfo, item, sliderIDInPage++, (IList<float>)array, index);
			}
			else if (type == typeof(byte))
			{
				e = new UIModConfigByteItem(memberInfo, item, sliderIDInPage++, (IList<byte>)array, index);
			}
			else if (type == typeof(uint))
			{
				e = new UIModConfigUIntItem(memberInfo, item, sliderIDInPage++, (IList<uint>)array, index);
			}
			else if (type == typeof(int))
			{
				e = new UIModConfigIntItem(memberInfo, item, sliderIDInPage++, (IList<int>)array, index);
			}
			else if (type == typeof(string))
			{
				OptionStringsAttribute ost = (OptionStringsAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(OptionStringsAttribute));
				if (ost != null)
				{
					e = new UIModConfigStringItem(memberInfo, item, sliderIDInPage++, (IList<string>)array, index);
				}
				else
				{
					// TODO: Text input? Necessary?
					//e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}). Missing OptionStringsAttribute.");
					e = new UIModConfigStringInputItem(memberInfo, item, (IList<string>)array, index);
					sliderIDInPage++;
				}
			}
			else if (type.IsEnum)
			{
				if (array != null)
				{
					e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
				}
				else
				{
					e = new UIModConfigEnumItem(memberInfo, item, sliderIDInPage++);
				}
			}
			else if (type.IsArray)
			{
				e = new UIModConfigArrayItem(memberInfo, item, ref sliderIDInPage);
				elementHeight = 225;
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				e = new UIModConfigListItem(memberInfo, item, ref sliderIDInPage);
				elementHeight = 225;
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				e = new UIModConfigDictionaryItem(memberInfo, item, ref sliderIDInPage);
				elementHeight = 300;
			}
			else if (type.IsClass)
			{
				sliderIDInPage++;
				if (array != null)
				{
					object listItem = ((IList)array)[index];
					if (listItem == null)
					{
						listItem = Activator.CreateInstance(type);
						JsonConvert.PopulateObject("{}", listItem, ConfigManager.serializerSettings);
						((IList)array)[index] = listItem;
					}
					e = new UIModConfigObjectItem(memberInfo, listItem, ref sliderIDInPage, (IList)array, index);
					elementHeight = (int)(e as UIModConfigObjectItem).GetHeight();
				}
				else
				{
					object subitem = memberInfo.GetValue(item);
					if (subitem == null)
					{
						subitem = Activator.CreateInstance(type);
						JsonConvert.PopulateObject("{}", subitem, ConfigManager.serializerSettings);

						//JsonDefaultValueAttribute jsonDefaultValueAttribute = (JsonDefaultValueAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(JsonDefaultValueAttribute));
						//if (jsonDefaultValueAttribute != null)
						//{
						//	JsonConvert.PopulateObject(jsonDefaultValueAttribute.json, subitem, ConfigManager.serializerSettings);
						//}

						memberInfo.SetValue(item, subitem);
					}
					SeparatePageAttribute att = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute));
					if (att != null)
					{
						UIPanel separateListPanel = MakeSeparateListPanel(subitem, memberInfo);

						string name = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, subitem, null)?.Label ?? memberInfo.Name;
						e = new UITextPanel<string>(name);
						e.HAlign = 0.5f;
						elementHeight = 40;
						e.OnClick += (a, c) =>
						{
							Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
							Interface.modConfig.uIElement.Append(separateListPanel);
							Interface.modConfig.configPanelStack.Push(separateListPanel);
							//separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

							//UIPanel panel = new UIPanel();
							//panel.Width.Set(200, 0);
							//panel.Height.Set(200, 0);
							//panel.Left.Set(200, 0);
							//panel.Top.Set(200, 0);
							//Interface.modConfig.Append(panel);

							//Interface.modConfig.subMenu.Enqueue(subitem);
							//Interface.modConfig.DoMenuModeState();
						};
						//e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
						//e.OnClick += (a, b) => { };
					}
					else
					{
						e = new UIModConfigObjectItem(memberInfo, subitem, ref sliderIDInPage);
						elementHeight = (int)(e as UIModConfigObjectItem).GetHeight();
					}
				}
			}
			else if (type.IsValueType && !type.IsPrimitive)
			{
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
				e.Top.Pixels += 6;
				e.Left.Pixels += 4;
			}
			else
			{
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
				e.Top.Pixels += 6;
				e.Left.Pixels += 4;
			}
			if (e != null)
			{
				var container = GetContainer(e, original);
				container.Height.Pixels = elementHeight;
				UIList list = parent as UIList;
				if (list != null)
				{
					list.Add(container);
				}
				else
				{
					container.Top.Pixels = top;
					container.Width.Pixels = -20;
					container.Left.Pixels = 20;
					top += elementHeight + 4; // or use list and padding?
					parent.Append(container);
				}

				return new Tuple<UIElement, UIElement>(container, e);
			}
			return null;
		}

		private static UIPanel MakeSeparateListPanel(object subitem, PropertyFieldWrapper memberInfo)
		{
			

			UIPanel uIPanel = new UIPanel();
			uIPanel.CopyStyle(Interface.modConfig.uIPanel);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;

			BackgroundColorAttribute bca = ConfigManager.GetCustomAttribute<BackgroundColorAttribute>(memberInfo, subitem, null);
			if (bca != null)
			{
				uIPanel.BackgroundColor = bca.color;
			}

			//uIElement.Append(uIPanel);

			UIList separateList = new UIList();
			separateList.CopyStyle(Interface.modConfig.mainConfigList);
			separateList.Height.Set(-40f, 1f);
			separateList.Top.Set(40f, 0f);
			uIPanel.Append(separateList);
			int i = 0;
			int top = 0;

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-40f, 1f);
			uIScrollbar.Top.Set(40f, 0f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			separateList.SetScrollbar(uIScrollbar);

			string name = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, subitem, null)?.Label ?? memberInfo.Name;
			Interface.modConfig.subPageStack.Push(name);
			//UIPanel heading = new UIPanel();
			//UIText headingText = new UIText(name);

			name = Interface.modConfig.subPageStack.Aggregate((current, next) => current + "/" + next);

			UITextPanel<string> heading = new UITextPanel<string>(name);
			heading.HAlign = 0f;
			//heading.Width.Set(-10, 0.5f);
			//heading.Left.Set(60, 0f);
			heading.Top.Set(-6, 0);
			heading.Height.Set(40, 0);
			//var headingContainer = GetContainer(heading, i++);
			//headingContainer.Height.Pixels = 40;
			uIPanel.Append(heading);
			//headingText.Top.Set(6, 0);
			//headingText.Left.Set(0, .5f);
			//headingText.HAlign = .5f;
			//uIPanel.Append(headingText);
			//top += 40;

			UITextPanel<string> back = new UITextPanel<string>("Back");
			back.HAlign = 1f;
			back.Width.Set(50, 0f);
			back.Top.Set(-6, 0);
			//top += 40;
			//var capturedCurrent = Interface.modConfig.currentConfigList;
			back.OnClick += (a, c) =>
			{
				Interface.modConfig.uIElement.RemoveChild(uIPanel);
				Interface.modConfig.configPanelStack.Pop();
				Interface.modConfig.uIElement.Append(Interface.modConfig.configPanelStack.Peek());
				//Interface.modConfig.configPanelStack.Peek().SetScrollbar(Interface.modConfig.uIScrollbar);
				//Interface.modConfig.currentConfigList = capturedCurrent;
			};
			back.OnMouseOver += UICommon.FadedMouseOver;
			back.OnMouseOut += UICommon.FadedMouseOut;
			//var backContainer = GetContainer(back, i++);
			//backContainer.Height.Pixels = 40;
			uIPanel.Append(back);

			//var b = new UIText("Test");
			//separateList.Add(b);
			// Make rest of list


			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem))
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
					continue;

				WrapIt(separateList, ref top, variable, subitem, ref i);
			}
			Interface.modConfig.subPageStack.Pop();
			return uIPanel;
		}

		internal static UIElement GetContainer(UIElement containee, int sortid)
		{
			UIElement container = new UISortableElement(sortid);
			container.Width.Set(0f, 1f);
			container.Height.Set(30f, 0f);
			//container.HAlign = 1f;
			container.Append(containee);
			return container;
		}
	}

	public class PropertyFieldWrapper
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
			{
				if (propertyInfo.CanWrite) // TODO: Grey out?
					propertyInfo.SetValue(obj, value, null);
			}
		}

		public bool CanWrite => fieldInfo != null ? true : propertyInfo.CanWrite;
	}

	abstract class UIConfigRangeItem : UIModConfigItem
	{
		internal bool drawTicks;
		public abstract int NumberTicks { get; }
		public abstract float TickIncrement { get; }
		protected Func<float> _GetProportion;
		protected Action<float> _SetProportion;
		private int _sliderIDInPage;

		public UIConfigRangeItem(int sliderIDInPage, PropertyFieldWrapper memberInfo, object item, IList array) : base(memberInfo, item, array)
		{
			this._sliderIDInPage = sliderIDInPage;
			drawTicks = Attribute.IsDefined(memberInfo.MemberInfo, typeof(DrawTicksAttribute));
		}

		public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null)
		{
			perc = Utils.Clamp(perc, -.05f, 1.05f);
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
						if (percent <= 1f)
							sb.Draw(Main.magicPixel, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
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

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			float num = 6f;
			int num2 = 0;
			IngameOptions.rightHover = -1;
			if (!Main.mouseLeft)
			{
				IngameOptions.rightLock = -1;
			}
			if (IngameOptions.rightLock == this._sliderIDInPage)
			{
				num2 = 1;
			}
			else if (IngameOptions.rightLock != -1)
			{
				num2 = 2;
			}
			CalculatedStyle dimensions = base.GetDimensions();
			float num3 = dimensions.Width + 1f;
			Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
			bool flag2 = base.IsMouseHovering;
			if (num2 == 1)
			{
				flag2 = true;
			}
			if (num2 == 2)
			{
				flag2 = false;
			}
			Vector2 vector2 = vector;
			vector2.X += 8f;
			vector2.Y += 2f + num;
			vector2.X -= 17f;
			Main.colorBarTexture.Frame(1, 1, 0, 0);
			vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
			IngameOptions.valuePosition = vector2;
			float obj = DrawValueBar(spriteBatch, 1f, this._GetProportion(), num2);
			if (IngameOptions.inBar || IngameOptions.rightLock == this._sliderIDInPage)
			{
				IngameOptions.rightHover = this._sliderIDInPage;
				if (PlayerInput.Triggers.Current.MouseLeft && IngameOptions.rightLock == this._sliderIDInPage)
				{
					this._SetProportion(obj);
				}
			}
			if (IngameOptions.rightHover != -1 && IngameOptions.rightLock == -1)
			{
				IngameOptions.rightLock = IngameOptions.rightHover;
			}
		}
	}
}
