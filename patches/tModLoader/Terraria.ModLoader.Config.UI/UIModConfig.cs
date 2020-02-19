using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	// TODO: Revert individual button.	
	// TODO: Collapse All button, or default to collapsed?
	// TODO: Localization support
	internal class UIModConfig : UIState
	{
		private UIElement uIElement;
		private UITextPanel<string> headerTextPanel;
		private UITextPanel<string> message;
		private UITextPanel<string> previousConfigButton;
		private UITextPanel<string> nextConfigButton;
		private UITextPanel<string> saveConfigButton;
		private UITextPanel<string> backButton;
		private UITextPanel<string> revertConfigButton;
		private UITextPanel<string> restoreDefaultsConfigButton;
		private UIPanel uIPanel;
		private readonly List<Tuple<UIElement, UIElement>> mainConfigItems = new List<Tuple<UIElement, UIElement>>();
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
		private ModConfig pendingConfig;
		public int updateCount;

		private bool updateNeeded;
		private UIFocusInputTextField filterTextField;

		public override void OnInitialize() {
			uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(160f, 0f);
			uIElement.Height.Set(-180f, 1f);
			uIElement.HAlign = 0.5f;

			uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-140f, 1f);
			uIPanel.Top.Set(30f, 0f);
			uIPanel.BackgroundColor = UICommon.MainPanelBackground;
			uIElement.Append(uIPanel);

			UIPanel textBoxBackground = new UIPanel();
			textBoxBackground.SetPadding(0);
			filterTextField = new UIFocusInputTextField("Filter Options");
			textBoxBackground.Top.Set(2f, 0f);
			textBoxBackground.Left.Set(-190, 1f);
			textBoxBackground.Width.Set(180, 0f);
			textBoxBackground.Height.Set(30, 0f);
			uIElement.Append(textBoxBackground);

			filterTextField.SetText("");
			filterTextField.Top.Set(5, 0f);
			filterTextField.Left.Set(10, 0f);
			filterTextField.Width.Set(-20, 1f);
			filterTextField.Height.Set(20, 0);
			filterTextField.OnTextChange += (a, b) => {
				updateNeeded = true;
			};
			filterTextField.OnRightClick += (a, b) => {
				filterTextField.SetText("");
			};
			textBoxBackground.Append(filterTextField);

			// TODO: ModConfig Localization support
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
			headerTextPanel.BackgroundColor = UICommon.DefaultUIBlue;
			uIElement.Append(headerTextPanel);

			previousConfigButton = new UITextPanel<string>("<", 1f, false);
			previousConfigButton.Width.Set(25f, 0);
			previousConfigButton.Height.Set(25f, 0f);
			previousConfigButton.VAlign = 1f;
			previousConfigButton.Top.Set(-65f, 0f);
			previousConfigButton.HAlign = 0f;
			previousConfigButton.WithFadedMouseOver();
			previousConfigButton.OnClick += PreviousConfig;
			//uIElement.Append(previousConfigButton);

			nextConfigButton = new UITextPanel<string>(">", 1f, false);
			nextConfigButton.CopyStyle(previousConfigButton);
			nextConfigButton.WithFadedMouseOver();
			nextConfigButton.HAlign = 1f;
			nextConfigButton.OnClick += NextConfig;
			//uIElement.Append(nextConfigButton);

			saveConfigButton = new UITextPanel<string>("Save Config", 1f, false);
			saveConfigButton.Width.Set(-10f, 1f / 4f);
			saveConfigButton.Height.Set(25f, 0f);
			saveConfigButton.Top.Set(-20f, 0f);
			saveConfigButton.WithFadedMouseOver();
			saveConfigButton.HAlign = 0.33f;
			saveConfigButton.VAlign = 1f;
			saveConfigButton.OnClick += SaveConfig;
			//uIElement.Append(saveConfigButton);

			backButton = new UITextPanel<string>("Back", 1f, false);
			backButton.CopyStyle(saveConfigButton);
			backButton.HAlign = 0;
			backButton.WithFadedMouseOver();
			backButton.OnMouseOver += (a, b) => {
				if (pendingChanges)
					backButton.BackgroundColor = Color.Red;
			};
			backButton.OnMouseOut += (a, b) => {
				if (pendingChanges)
					backButton.BackgroundColor = Color.Red * 0.7f;
			};
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			revertConfigButton = new UITextPanel<string>("Revert Changes", 1f, false);
			revertConfigButton.CopyStyle(saveConfigButton);
			revertConfigButton.WithFadedMouseOver();
			revertConfigButton.HAlign = 0.66f;
			revertConfigButton.OnClick += RevertConfig;
			//uIElement.Append(revertConfigButton);

			//float scale = Math.Min(1f, 130f/Main.fontMouseText.MeasureString("Restore Defaults").X);
			restoreDefaultsConfigButton = new UITextPanel<string>("Restore Defaults", 1f, false);
			restoreDefaultsConfigButton.CopyStyle(saveConfigButton);
			restoreDefaultsConfigButton.WithFadedMouseOver();
			restoreDefaultsConfigButton.HAlign = 1f;
			restoreDefaultsConfigButton.OnClick += RestoreDefaults;
			uIElement.Append(restoreDefaultsConfigButton);

			uIPanel.BackgroundColor = UICommon.MainPanelBackground;

			Append(uIElement);
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modsMenuID;

			//Main.menuMode = 1127;
			if (!Main.gameMenu) {
				Main.InGameUI.SetState(Interface.modConfigList);
			}
			//IngameFancyUI.Close();
			//if (ConfigManager.ModNeedsReload(mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
		}

		internal void Unload() {
			mainConfigList?.Clear();
			mainConfigItems?.Clear();
			mod = null;
			modConfigs = null;
			modConfig = null;
			pendingConfig = null;
			while (configPanelStack.Count > 1)
				uIElement.RemoveChild(configPanelStack.Pop());
		}

		// TODO: with in-game version, disable ConfigScope.ServerSide configs (View Only maybe?)
		private void PreviousConfig(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			//DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index - 1 < 0 ? modConfigs.Count - 1 : index - 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		private void NextConfig(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			//DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index + 1 > modConfigs.Count ? 0 : index + 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		// Refreshes the UI to refresh recent changes such as Save/Discard/Restore Defaults/Cycle to next config
		private void DoMenuModeState() {
			if (Main.gameMenu)
				Main.menuMode = Interface.modConfigID;
			else
				Main.InGameUI.SetState(Interface.modConfig);
		}

		private void SaveConfig(UIMouseEvent evt, UIElement listeningElement) {
			// Main Menu: Save, leave reload for later
			// MP with ServerSide: Send request to server
			// SP or MP with ClientSide: Apply immediately if !NeedsReload
			if (Main.gameMenu) {
				Main.PlaySound(SoundID.MenuOpen);
				ConfigManager.Save(pendingConfig);
				ConfigManager.Load(modConfig);
				// modConfig.OnChanged(); delayed until ReloadRequired checked
				// Reload will be forced by Back Button in UIMods if needed
			}
			else {
				// If we are in game...
				if (pendingConfig.Mode == ConfigScope.ServerSide && Main.netMode == NetmodeID.MultiplayerClient) {
					// TODO: Too 
					SetMessage("Asking server to accept changes...", Color.Yellow);

					var requestChanges = new ModPacket(MessageID.InGameChangeConfig);
					requestChanges.Write(pendingConfig.mod.Name);
					requestChanges.Write(pendingConfig.Name);
					string json = JsonConvert.SerializeObject(pendingConfig, ConfigManager.serializerSettingsCompact);
					requestChanges.Write(json);
					requestChanges.Send();

					//IngameFancyUI.Close();

					return;
				}
				// SP or MP with ClientSide
				ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.mod, modConfig.Name);
				if (loadTimeConfig.NeedsReload(pendingConfig)) {
					Main.PlaySound(SoundID.MenuClose);
					SetMessage("Can't save because changes would require a reload.", Color.Red);
					return;
				}
				else {
					Main.PlaySound(SoundID.MenuOpen);
					ConfigManager.Save(pendingConfig);
					ConfigManager.Load(modConfig);
					modConfig.OnChanged();
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
		}

		private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			pendingRevertDefaults = true;
			DoMenuModeState();
		}

		private void RevertConfig(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			DiscardChanges();
		}

		private void DiscardChanges() {
			DoMenuModeState();
		}

		bool pendingChanges;
		bool pendingChangesUIUpdate;
		public void SetPendingChanges(bool changes = true) {
			pendingChangesUIUpdate |= changes;
			pendingChanges |= changes;
		}

		public void SetMessage(string text, Color color) {
			message.TextScale = 1f;
			message.SetText("Notification: " + text);
			float width = Main.fontMouseText.MeasureString(text).X;
			if (width > 400) {
				message.TextScale = 400 / width;
				message.Recalculate();
			}
			message.TextColor = color;
		}

		bool netUpdate;
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			updateCount++;
			if (pendingChangesUIUpdate) {
				uIElement.Append(saveConfigButton);
				uIElement.Append(revertConfigButton);
				backButton.BackgroundColor = Color.Red * 0.7f;
				pendingChangesUIUpdate = false;
			}
			if (netUpdate) {
				DoMenuModeState();
				netUpdate = false;
			}
			if (!updateNeeded) return;
			updateNeeded = false;
			mainConfigList.Clear();
			mainConfigList.AddRange(mainConfigItems.Where(item => {
				if (item.Item2 is ConfigElement configElement) {
					return configElement.TextDisplayFunction().IndexOf(filterTextField.CurrentString, StringComparison.OrdinalIgnoreCase) != -1;
				}
				return true;
			}).Select(x=>x.Item1));
			Recalculate();
		}

		public static string tooltip;
		public override void Draw(SpriteBatch spriteBatch) {
			tooltip = null;
			base.Draw(spriteBatch);
			if (!string.IsNullOrEmpty(tooltip)) {
				UICommon.DrawHoverStringInBounds(spriteBatch, tooltip, GetDimensions().ToRectangle());
			}
		}

		// do we need 2 copies? We can discard changes by reloading.
		// We can save pending changes by saving file then loading/reloading mods.
		// when we get new server configs from server...replace, don't save?
		// reload manually, reload fresh server config?
		// need some CopyTo method to preserve references....hmmm
		internal void SetMod(Mod mod, ModConfig config = null) {
			this.mod = mod;
			if (ConfigManager.Configs.ContainsKey(mod)) {
				modConfigs = ConfigManager.Configs[mod];
				modConfig = modConfigs[0];
				if (config != null) {
					modConfig = ConfigManager.Configs[mod].First(x => x == config);
					// TODO, decide which configs to show in game: modConfigs = ConfigManager.Configs[mod].Where(x => x.Mode == ConfigScope.ClientSide).ToList();
				}
				//modConfigClone = modConfig.Clone();

				// if in game, maybe have all configs open

			}
			else {
				throw new Exception($"There are no ModConfig for {mod.DisplayName}, how did this happen?");
			}
		}

		static bool pendingRevertDefaults;
		public override void OnActivate() {
			filterTextField.SetText("");
			updateNeeded = false;
			SetMessage("", Color.White);
			string configDisplayName = ((LabelAttribute)Attribute.GetCustomAttribute(modConfig.GetType(), typeof(LabelAttribute)))?.Label ?? modConfig.Name;
			headerTextPanel.SetText(string.IsNullOrEmpty(configDisplayName) ? modConfig.mod.DisplayName : modConfig.mod.DisplayName + ": " + configDisplayName);
			pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
			pendingChanges = pendingRevertDefaults;
			if (pendingRevertDefaults) {
				pendingRevertDefaults = false;
				ConfigManager.Reset(pendingConfig);
				pendingChangesUIUpdate = true;
			}

			int index = modConfigs.IndexOf(modConfig);
			int count = modConfigs.Count;
			//pendingChanges = false;
			backButton.BackgroundColor = UICommon.DefaultUIBlueMouseOver;
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
			mainConfigItems.Clear();
			mainConfigList.Clear();
			configPanelStack.Clear();
			configPanelStack.Push(uIPanel);
			subPageStack.Clear();
			//currentConfigList = mainConfigList;
			int top = 0;
			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			uIPanel.BackgroundColor = UICommon.MainPanelBackground;
			var backgroundColorAttribute = (BackgroundColorAttribute)Attribute.GetCustomAttribute(pendingConfig.GetType(), typeof(BackgroundColorAttribute));
			if (backgroundColorAttribute != null) {
				uIPanel.BackgroundColor = backgroundColorAttribute.color;
			}

			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(pendingConfig)) {
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
					continue;
				HeaderAttribute header = ConfigManager.GetCustomAttribute<HeaderAttribute>(variable, null, null);
				if (header != null) {
					var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
					WrapIt(mainConfigList, ref top, wrapper, header, order++);
				}
				WrapIt(mainConfigList, ref top, variable, pendingConfig, order++);
			}
		}

		public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1) {
			int elementHeight = 0;
			Type type = memberInfo.Type;
			if (arrayType != null) {
				type = arrayType;
			}
			UIElement e;

			// TODO: Other common structs? -- Rectangle, Point
			CustomModConfigItemAttribute customUI = ConfigManager.GetCustomAttribute<CustomModConfigItemAttribute>(memberInfo, null, null);
			if (customUI != null) {
				Type customUIType = customUI.t;
				if (typeof(ConfigElement).IsAssignableFrom(customUIType)) {
					ConstructorInfo ctor = customUIType.GetConstructor(new Type[0]);
					if (ctor != null) {
						object instance = ctor.Invoke(new object[0]);
						e = instance as UIElement;
					}
					else {
						e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not have an empty constructor.");
					}
				}
				else {
					e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not inherit from ConfigElement.");
				}
			}
			else if (item.GetType() == typeof(HeaderAttribute)) {
				e = new HeaderElement((string)memberInfo.GetValue(item));
			}
			else if (type == typeof(ItemDefinition)) {
				e = new ItemDefinitionElement();
			}
			else if (type == typeof(ProjectileDefinition)) {
				e = new ProjectileDefinitionElement();
			}
			else if (type == typeof(NPCDefinition)) {
				e = new NPCDefinitionElement();
			}
			else if (type == typeof(PrefixDefinition)) {
				e = new PrefixDefinitionElement();
			}
			else if (type == typeof(Color)) {
				e = new ColorElement();
			}
			else if (type == typeof(Vector2)) {
				e = new Vector2Element();
			}
			else if (type == typeof(bool)) // isassignedfrom?
			{
				e = new BooleanElement();
			}
			else if (type == typeof(float)) {
				e = new FloatElement();
			}
			else if (type == typeof(byte)) {
				e = new ByteElement();
			}
			else if (type == typeof(uint)) {
				e = new UIntElement();
			}
			else if (type == typeof(int)) {
				SliderAttribute sliderAttribute = ConfigManager.GetCustomAttribute<SliderAttribute>(memberInfo, item, list);
				if (sliderAttribute != null)
					e = new IntRangeElement();
				else
					e = new IntInputElement();
			}
			else if (type == typeof(string)) {
				OptionStringsAttribute ost = ConfigManager.GetCustomAttribute<OptionStringsAttribute>(memberInfo, item, list);
				if (ost != null)
					e = new StringOptionElement();
				else
					e = new StringInputElement();
			}
			else if (type.IsEnum) {
				if (list != null)
					e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
				else
					e = new EnumElement();
			}
			else if (type.IsArray) {
				e = new ArrayElement();
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
				e = new ListElement();
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>)) {
				e = new SetElement();
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				e = new DictionaryElement();
			}
			else if (type.IsClass) {
				e = new ObjectElement(/*, ignoreSeparatePage: ignoreSeparatePage*/);
			}
			else if (type.IsValueType && !type.IsPrimitive) {
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
				//e.Top.Pixels += 6;
				e.Height.Pixels += 6;
				e.Left.Pixels += 4;

				object subitem = memberInfo.GetValue(item);
			}
			else {
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
				e.Top.Pixels += 6;
				e.Left.Pixels += 4;
			}
			if (e != null) {
				if (e is ConfigElement configElement) {
					configElement.Bind(memberInfo, item, (IList)list, index);
					configElement.OnBind();
				}
				e.Recalculate();
				elementHeight = (int)e.GetOuterDimensions().Height;

				var container = GetContainer(e, index == -1 ? order : index);
				container.Height.Pixels = elementHeight;
				UIList uiList = parent as UIList;
				if (uiList != null) {
					uiList.Add(container);
					float p = uiList.GetTotalHeight();
				}
				else {
					// Only Vector2 and Color use this I think, but modders can use the non-UIList approach for custom UI and layout.
					container.Top.Pixels = top;
					container.Width.Pixels = -20;
					container.Left.Pixels = 20;
					top += elementHeight + 4;
					parent.Append(container);
					parent.Height.Set(top, 0);
				}
				var tuple = new Tuple<UIElement, UIElement>(container, e);
				if(parent == Interface.modConfig.mainConfigList) {
					Interface.modConfig.mainConfigItems.Add(tuple);
				}

				return tuple;
			}
			return null;
		}

		internal static UIElement GetContainer(UIElement containee, int sortid) {
			UIElement container = new UISortableElement(sortid);
			container.Width.Set(0f, 1f);
			container.Height.Set(30f, 0f);
			//container.HAlign = 1f;
			container.Append(containee);
			return container;
		}

		internal static UIPanel MakeSeparateListPanel(object item, object subitem, PropertyFieldWrapper memberInfo, IList array, int index, Func<string> AbridgedTextDisplayFunction) {
			UIPanel uIPanel = new UIPanel();
			uIPanel.CopyStyle(Interface.modConfig.uIPanel);
			uIPanel.BackgroundColor = UICommon.MainPanelBackground;

			BackgroundColorAttribute bca = ConfigManager.GetCustomAttribute<BackgroundColorAttribute>(memberInfo, subitem, null);
			if (bca != null) {
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
			if (index != -1)
				name = name + " #" + (index + 1);
			Interface.modConfig.subPageStack.Push(name);
			//UIPanel heading = new UIPanel();
			//UIText headingText = new UIText(name);

			name = string.Join(" > ", Interface.modConfig.subPageStack.Reverse()); //.Aggregate((current, next) => current + "/" + next);

			UITextPanel<string> heading = new UITextPanel<string>(name); // TODO: ToString as well. Separate label?
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
			back.OnClick += (a, c) => {
				Interface.modConfig.uIElement.RemoveChild(uIPanel);
				Interface.modConfig.configPanelStack.Pop();
				Interface.modConfig.uIElement.Append(Interface.modConfig.configPanelStack.Peek());
				//Interface.modConfig.configPanelStack.Peek().SetScrollbar(Interface.modConfig.uIScrollbar);
				//Interface.modConfig.currentConfigList = capturedCurrent;
			};
			back.WithFadedMouseOver();
			//var backContainer = GetContainer(back, i++);
			//backContainer.Height.Pixels = 40;
			uIPanel.Append(back);

			//var b = new UIText("Test");
			//separateList.Add(b);
			// Make rest of list


			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			if (true) {
				int order = 0;
				bool hasToString = false;
				if (array != null) {
					var listType = memberInfo.Type.GetGenericArguments()[0];
					hasToString = listType.GetMethod("ToString", new Type[0]).DeclaringType != typeof(object);
				}
				else {
					hasToString = memberInfo.Type.GetMethod("ToString", new Type[0]).DeclaringType != typeof(object);
				}
				if (AbridgedTextDisplayFunction != null) {
					var display = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper() { func = AbridgedTextDisplayFunction, }) { DrawPanel = true };
					display.Recalculate();
					var container = GetContainer(display, order++);
					container.Height.Pixels = (int)display.GetOuterDimensions().Height;
					separateList.Add(container);
				}
				//if (hasToString)
				//	_TextDisplayFunction = () => index + 1 + ": " + (array[index]?.ToString() ?? "null");
				foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem)) {
					if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
						continue;
					HeaderAttribute header = ConfigManager.GetCustomAttribute<HeaderAttribute>(variable, null, null);
					if (header != null) {
						var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
						WrapIt(separateList, ref top, wrapper, header, order++);
					}
					WrapIt(separateList, ref top, variable, subitem, order++);
				}
			}
			else {
				//ignoreSeparatePage just to simplify ToString label--> had some issues.
				//WrapIt(separateList, ref top, memberInfo, item, 1, ignoreSeparatePage: true);
			}

			Interface.modConfig.subPageStack.Pop();
			return uIPanel;
		}

		internal static void SwitchToSubConfig(UIPanel separateListPanel) {
			Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
			Interface.modConfig.uIElement.Append(separateListPanel);
			Interface.modConfig.configPanelStack.Push(separateListPanel);
		}

		//public override void Recalculate()
		//{
		//	base.Recalculate();
		//	mainConfigList?.Recalculate();
		//}
	}
}
