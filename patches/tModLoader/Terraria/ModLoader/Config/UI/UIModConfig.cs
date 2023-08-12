using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.Localization;
using Terraria.GameContent;

namespace Terraria.ModLoader.Config.UI;

// TODO: Revert individual button.
internal class UIModConfig : UIState
{
	public int UpdateCount { get; set; }

	public static string Tooltip { get; set; }
	private static bool pendingRevertDefaults;

	private UIElement uIElement;
	private UIPanel uIPanel;
	private UIList mainConfigList;
	private UIScrollbar uIScrollbar;
	private UIFocusInputTextField filterTextField;
	private UITextPanel<LocalizedText> headerTextPanel;
	private UIAutoScaleTextTextPanel<LocalizedText> saveConfigButton;
	private UIAutoScaleTextTextPanel<LocalizedText> backButton;
	private UIAutoScaleTextTextPanel<LocalizedText> revertConfigButton;
	private UIAutoScaleTextTextPanel<LocalizedText> restoreDefaultsConfigButton;
	private UIPanel notificationModal;
	private UIText notificationModalText;
	private UIText notificationModalHeader;
	private UIImage modalInputBlocker;

	private readonly List<Tuple<UIElement, UIElement>> mainConfigItems = new();
	private readonly Stack<UIPanel> configPanelStack = new();
	private readonly Stack<string> subPageStack = new();

	private Mod mod;
	private List<ModConfig> modConfigs;
	private ModConfig modConfig; // This is from ConfigManager.Configs
	internal ModConfig pendingConfig; // The clone we modify.
	private bool updateNeeded;
	private bool openedFromModder = false;
	private bool pendingChanges;
	private bool pendingChangesUIUpdate;
	private bool netUpdate;

	public override void OnInitialize()
	{
		uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f,
		};
		Append(uIElement);

		uIPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -65, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
		};
		uIElement.Append(uIPanel);

		headerTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigModConfig"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uIElement.Append(headerTextPanel);

		UIPanel textBoxBackground = new UIPanel {
			Width = { Pixels = 175 },
			Height = { Pixels = 30 },
			Top = { Pixels = 10 },
			HAlign = 1f,
		}.WithPadding(0f);
		uIPanel.Append(textBoxBackground);
		configPanelStack.Push(uIPanel);

		filterTextField = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterOptions")) {// TODO: localize
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		filterTextField.OnTextChange += (a, b) => updateNeeded = true;
		filterTextField.OnRightClick += (a, b) => filterTextField.SetText("");
		filterTextField.SetText("");
		textBoxBackground.Append(filterTextField);

		float listTop = 50;
		mainConfigList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -listTop, Percent = 1f },
			Top = { Pixels = listTop },
			ListPadding = 5f,
		};
		uIPanel.Append(mainConfigList);

		uIScrollbar = new UIScrollbar {
			Top = { Pixels = listTop },
			Height = { Pixels = -listTop, Percent = 1f },
			HAlign = 1f,
		};
		uIScrollbar.SetView(100f, 1000f);
		mainConfigList.SetScrollbar(uIScrollbar);
		uIPanel.Append(uIScrollbar);

		backButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigBack")) {
			Width = { Pixels = -10, Percent = 0.25f },
			Height = { Pixels = 40 },
			Top = { Pixels = -20 },
			HAlign = 0 / 3f,
			VAlign = 1f,
		}.WithFadedMouseOver();
		backButton.OnMouseOver += (a, b) => {
			if (pendingChanges)
				backButton.BackgroundColor = Color.Red;
		};
		backButton.OnMouseOut += (a, b) => {
			if (pendingChanges)
				backButton.BackgroundColor = Color.Red * 0.7f;
		};
		backButton.OnLeftClick += BackClick;
		uIElement.Append(backButton);

		saveConfigButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig"));
		saveConfigButton.CopyStyle(backButton);
		saveConfigButton.WithFadedMouseOver();
		saveConfigButton.HAlign = 1 / 3f;
		saveConfigButton.OnLeftClick += SaveConfig;
		// Don't append

		revertConfigButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigRevertChanges"));
		revertConfigButton.CopyStyle(backButton);
		revertConfigButton.WithFadedMouseOver();
		revertConfigButton.HAlign = 2 / 3f;
		revertConfigButton.OnLeftClick += RevertConfig;
		// Don't append

		restoreDefaultsConfigButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigRestoreDefaults"));
		restoreDefaultsConfigButton.CopyStyle(backButton);
		restoreDefaultsConfigButton.WithFadedMouseOver();
		restoreDefaultsConfigButton.HAlign = 3 / 3f;
		restoreDefaultsConfigButton.OnLeftClick += RestoreDefaults;
		uIElement.Append(restoreDefaultsConfigButton);

		notificationModal = new UIPanel {
			Width = { Pixels = 400 },
			Height = { Pixels = 200 },
			HAlign = 0.5f,
			VAlign = 0.5f,
			BackgroundColor = UICommon.DefaultUIBlue,
		};
		// Don't append

		notificationModalText = new UIText("") {
			Width = { Percent = 1f },
			MaxHeight = { Pixels = 175 },
			HAlign = 0.5f,
			VAlign = 0.5f,
			IsWrapped = true,
		};
		notificationModal.Append(notificationModalText);

		notificationModalHeader = new UIText("", 0.5f, large: true) {
			HAlign = 0.5f,
		};
		notificationModal.Append(notificationModalHeader);

		var modalCloseButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
		};
		modalCloseButton.OnLeftClick += ClearMessage;
		notificationModal.Append(modalCloseButton);

		modalInputBlocker = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = new Color(0, 0, 0, 0),
			ScaleToFit = true,
		};
		modalInputBlocker.OnLeftClick += ClearMessage;
		// Don't append
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);

		if (!mainConfigList.ContainsPoint(Main.MouseScreen))
			uIScrollbar.ViewPosition -= evt.ScrollWheelValue;
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);

		if (Main.gameMenu) {
			Main.menuMode = openedFromModder ? MenuID.Title : Interface.modConfigListID;
		}
		else {
			if (openedFromModder)
				IngameFancyUI.Close();
			else
				Main.InGameUI.SetState(Interface.modConfigList);
		}
	}

	internal void Unload()
	{
		mainConfigList?.Clear();
		mainConfigItems?.Clear();
		mod = null;
		modConfigs = null;
		modConfig = null;
		pendingConfig = null;

		while (configPanelStack.Count > 1)
			uIElement.RemoveChild(configPanelStack.Pop());
	}

	// Refreshes the UI to refresh recent changes such as Save/Discard/Restore Defaults
	public void DoMenuModeState()
	{
		if (Main.gameMenu) {
			Main.MenuUI.SetState(null);
			Main.menuMode = Interface.modConfigID;
		}
		else {
			Main.InGameUI.SetState(null);
			Main.InGameUI.SetState(Interface.modConfig);
		}
	}

	private void SaveConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		// Main Menu: Save, leave reload for later
		// MP with ServerSide: Send request to server
		// SP or MP with ClientSide: Apply immediately if !NeedsReload
		if (Main.gameMenu) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			ConfigManager.Save(pendingConfig);
			ConfigManager.Load(modConfig);
			// modConfig.OnChanged(); delayed until ReloadRequired checked
			// Reload will be forced by Back Button in UIMods if needed
		}
		else {
			// If we are in game...
			if (pendingConfig.Mode == ConfigScope.ServerSide && Main.netMode == NetmodeID.MultiplayerClient) {
				SetMessage(Language.GetTextValue("tModLoader.ModConfigAskingServerToAcceptChanges"), Language.GetTextValue("tModLoader.ModConfigChangesPending"), Color.Yellow);

				var requestChanges = new ModPacket(MessageID.InGameChangeConfig);
				requestChanges.Write(pendingConfig.Mod.Name);
				requestChanges.Write(pendingConfig.Name);
				string json = JsonConvert.SerializeObject(pendingConfig, ConfigManager.serializerSettingsCompact);
				requestChanges.Write(json);
				requestChanges.Send();

				//IngameFancyUI.Close();

				return;
			}

			// SP or MP with ClientSide
			ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);

			if (loadTimeConfig.NeedsReload(pendingConfig)) {
				SetMessage(Language.GetTextValue("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload"), Language.GetTextValue("tModLoader.ModConfigChangesRejected"), Color.Red);
				return;
			}
			else {
				SoundEngine.PlaySound(SoundID.MenuOpen);
				ConfigManager.Save(pendingConfig);
				ConfigManager.Load(modConfig);
				modConfig.OnChanged();
			}
		}

		/*
		if (ConfigManager.ModNeedsReload(modConfig.mod)) {
			Main.menuMode = Interface.reloadModsID;
		}
		else {
			DoMenuModeState();
		}
		*/

		DoMenuModeState();
	}

	private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		pendingRevertDefaults = true;
		DoMenuModeState();
	}

	private void RevertConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		DiscardChanges();
	}

	private void DiscardChanges()
	{
		DoMenuModeState();
	}

	// TODO: make this set pending changes based on whether elements were changed rather than interacted with, since changing their values back doesn't update the UI
	public void SetPendingChanges(bool changes = true)
	{
		pendingChangesUIUpdate |= changes;
		pendingChanges |= changes;
	}

	private void ClearMessage(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		SetMessage("", "");
	}

	public void SetMessage(string text, string header, Color color = default)
	{
		if (color == default)
			color = Color.White;

		RemoveChild(notificationModal);
		RemoveChild(modalInputBlocker);
		notificationModalText.SetText("");
		notificationModalHeader.SetText("");
		notificationModalHeader.TextColor = Color.White;

		if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(header)) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		Append(modalInputBlocker);
		Append(notificationModal);
		notificationModalText.SetText(text);
		notificationModalHeader.SetText(header);
		notificationModalHeader.TextColor = color;
	}

	public override void Update(GameTime gameTime)
	{
		UpdateCount++;

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

		base.Update(gameTime);

		if (revertConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRevertChangesHover"));

		if (restoreDefaultsConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRestoreDefaultsHover"));

		if (!updateNeeded)
			return;

		updateNeeded = false;

		mainConfigList.Clear();

		mainConfigList.AddRange(mainConfigItems.Where(item => {
			if (item.Item2 is ConfigElement configElement) {
				return configElement.TextDisplayFunction().IndexOf(filterTextField.CurrentString, StringComparison.OrdinalIgnoreCase) != -1;
			}
			return true;
		}).Select(x => x.Item1));

		Recalculate();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Tooltip = null;

		base.Draw(spriteBatch);

		if (!string.IsNullOrEmpty(Tooltip)) {
			UICommon.TooltipMouseText(Tooltip);
		}

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
	}

	// do we need 2 copies? We can discard changes by reloading.
	// We can save pending changes by saving file then loading/reloading mods.
	// when we get new server configs from server...replace, don't save?
	// reload manually, reload fresh server config?
	// need some CopyTo method to preserve references....hmmm
	internal void SetMod(Mod mod, ModConfig config = null, bool openedFromModder = false)
	{
		this.openedFromModder = openedFromModder;
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

	public override void OnActivate()
	{
		filterTextField.SetText("");
		SetMessage("", "", Color.White);
		updateNeeded = false;

		headerTextPanel.SetText(Language.GetText(string.Format("{0} - {1}", mod.DisplayName, modConfig.DisplayName.Value)));// Hacky, but works
		pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
		pendingChanges = pendingRevertDefaults;

		if (pendingRevertDefaults) {
			pendingRevertDefaults = false;
			ConfigManager.Reset(pendingConfig);
			pendingChangesUIUpdate = true;
		}

		backButton.BackgroundColor = UICommon.DefaultUIBlueMouseOver;
		uIElement.RemoveChild(saveConfigButton);
		uIElement.RemoveChild(revertConfigButton);
		uIElement.RemoveChild(configPanelStack.Peek());
		uIElement.Append(uIPanel);
		uIElement.RemoveChild(headerTextPanel);
		uIElement.Append(headerTextPanel);

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
			uIPanel.BackgroundColor = backgroundColorAttribute.Color;
		}

		int order = 0;

		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(pendingConfig)) {
			if (variable.IsProperty && variable.Name == "Mode")
				continue;

			if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
				continue;

			HandleHeader(mainConfigList, ref top, ref order, variable);

			WrapIt(mainConfigList, ref top, variable, pendingConfig, order++);
		}
	}

	public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
	{
		int elementHeight;
		Type type = memberInfo.Type;

		if (arrayType != null) {
			type = arrayType;
		}

		UIElement e;

		// TODO: Other common structs? -- Rectangle, Point
		var customUI = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomModConfigItemAttribute>(memberInfo, null, null);

		if (customUI != null) {
			Type customUIType = customUI.Type;

			if (typeof(ConfigElement).IsAssignableFrom(customUIType)) {
				ConstructorInfo ctor = customUIType.GetConstructor(Array.Empty<Type>());

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
			SliderAttribute sliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderAttribute>(memberInfo, item, list);

			if (sliderAttribute != null)
				e = new IntRangeElement();
			else
				e = new IntInputElement();
		}
		else if (type == typeof(string)) {
			OptionStringsAttribute ost = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(memberInfo, item, list);
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

			//object subitem = memberInfo.GetValue(item);
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

			if (parent is UIList uiList) {
				uiList.Add(container);
				uiList.GetTotalHeight();
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

			if (parent == Interface.modConfig.mainConfigList) {
				Interface.modConfig.mainConfigItems.Add(tuple);
			}

			return tuple;
		}
		return null;
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

	internal static UIPanel MakeSeparateListPanel(object item, object subitem, PropertyFieldWrapper memberInfo, IList array, int index, Func<string> AbridgedTextDisplayFunction)
	{
		UIPanel uIPanel = new UIPanel();
		uIPanel.CopyStyle(Interface.modConfig.uIPanel);
		uIPanel.BackgroundColor = UICommon.MainPanelBackground;

		BackgroundColorAttribute bca = ConfigManager.GetCustomAttributeFromMemberThenMemberType<BackgroundColorAttribute>(memberInfo, subitem, null);

		if (bca != null) {
			uIPanel.BackgroundColor = bca.Color;
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

		string name = ConfigManager.GetLocalizedLabel(memberInfo);
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

		UITextPanel<string> back = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModConfigBack")) {
			HAlign = 1f
		};

		back.Width.Set(50, 0f);
		back.Top.Set(-6, 0);

		//top += 40;
		//var capturedCurrent = Interface.modConfig.currentConfigList;

		back.OnLeftClick += (a, c) => {
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
				var display = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper(AbridgedTextDisplayFunction)) { DrawPanel = true };
				display.Recalculate();
				var container = GetContainer(display, order++);
				container.Height.Pixels = (int)display.GetOuterDimensions().Height;
				separateList.Add(container);
			}

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem)) {
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
					continue;

				HandleHeader(separateList, ref top, ref order, variable);

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

	public static void HandleHeader(UIElement parent, ref int top, ref int order, PropertyFieldWrapper variable)
	{
		HeaderAttribute header = ConfigManager.GetLocalizedHeader(variable.MemberInfo);

		if (header != null) {
			var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
			WrapIt(parent, ref top, wrapper, header, order++);
		}
	}

	internal static void SwitchToSubConfig(UIPanel separateListPanel)
	{
		Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
		Interface.modConfig.uIElement.Append(separateListPanel);
		Interface.modConfig.configPanelStack.Push(separateListPanel);
	}
}
