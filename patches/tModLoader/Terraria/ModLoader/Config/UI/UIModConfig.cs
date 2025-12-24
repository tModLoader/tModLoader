using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.Localization;
using tModPorter;

namespace Terraria.ModLoader.Config.UI;

// TODO: Revert individual button.
// TODO: Collapse All button, or default to collapsed?
// TODO: Localization support
public class UIModConfig : UIState, IHaveBackButtonCommand
{
	public int UpdateCount { get; set; }

	private readonly List<Tuple<UIElement, UIElement>> mainConfigItems = new();
	private BlockInputElement blockInput;
	private UIElement activeDialog;
	private readonly Stack<UIPanel> configPanelStack = new();
	private readonly Stack<string> subPageStack = new();
	//private UIList currentConfigList;
	private Mod mod;
	private List<ModConfig> sortedModConfigs; // NOT in load order. Don't use for anything other than navigation
	private ModConfig modConfig; // This is from ConfigManager.Configs
	internal ModConfig pendingConfig; // the clone we modify.
	private bool updateNeeded;
	private bool preserveNotificationMessage;
	internal string scrollToOption = null;
	internal bool centerScrolledOption = false;

	private bool openedFromModder = false;
	private Action modderOnClose = null;

	// TODO: these should be set in PendingChanges (which should be renamed to OnConfigModified) by comparing this config to the actual one/load time one
	public bool HasUnsavedChanges { get; set; } = false;// TODO: calculate properly, requires comparing configs
	public bool HasDefaultValues => false; // TODO: set/calculate properly, requires comparing configs

	#region UI

	private UIElement uiElement;
	private UIPanel uiPanel;
	private UITextPanel<string> headerTextPanel;

	private UIButton<LocalizedText> backButton;
	private UIButton<LocalizedText> saveConfigButton;
	private UIButton<LocalizedText> revertConfigButton;
	private UIButton<LocalizedText> restoreDefaultsConfigButton;

	private UIList mainConfigList;
	private UIScrollbar scrollbar;
	private UIFocusInputTextField filterTextField;

	// TODO: in future, all of the UI methods will be protected and virtual to allow modders to customize their UIState if they wish

	public override void OnInitialize()
	{
		CreateMainPanel();
		CreateButtons();
		CreatePanelContents();
		CreateHeaderPanel();
	}

	private void CreateMainPanel()
	{
		uiElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f,
		};
		Append(uiElement);

		uiPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -65, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
		};
		uiElement.Append(uiPanel);
	}

	private void CreateButtons()
	{
		backButton = CreateButton("tModLoader.ModConfigBack", 0, BackClick);
		backButton.UseAltColors = () => HasUnsavedChanges;
		backButton.AltPanelColor = Color.Red * 0.7f;
		backButton.AltHoverPanelColor = Color.Red;
		uiElement.Append(backButton);

		saveConfigButton = CreateButton("tModLoader.ModConfigSaveConfig", 1f / 3f, SaveConfig);
		saveConfigButton.UseAltColors = () => !HasUnsavedChanges;
		saveConfigButton.AltHoverText = Language.GetText("tModLoader.ModConfigSaveConfigNoUnsavedChanges");
		SetButtonAltColorsToDisabled(saveConfigButton);
		uiElement.Append(saveConfigButton);

		revertConfigButton = CreateButton("tModLoader.ModConfigRevertChanges", 2f / 3f, RevertConfig);
		revertConfigButton.HoverText = Language.GetText("tModLoader.ModConfigRevertChangesTooltip");
		revertConfigButton.UseAltColors = () => !HasUnsavedChanges;
		revertConfigButton.AltHoverText = Language.GetText("tModLoader.ModConfigRevertChangesNoUnsavedChanges");
		SetButtonAltColorsToDisabled(revertConfigButton);
		uiElement.Append(revertConfigButton);

		restoreDefaultsConfigButton = CreateButton("tModLoader.ModConfigRestoreDefaults", 1f, RestoreDefaults);
		restoreDefaultsConfigButton.HoverText = Language.GetText("tModLoader.ModConfigRestoreDefaultsTooltip");
		restoreDefaultsConfigButton.UseAltColors = () => HasDefaultValues;
		restoreDefaultsConfigButton.AltHoverText = Language.GetText("tModLoader.ModConfigRestoreDefaultsAlreadyDefault");
		SetButtonAltColorsToDisabled(restoreDefaultsConfigButton);
		uiElement.Append(restoreDefaultsConfigButton);
	}

	private static UIButton<LocalizedText> CreateButton(string localizationKey, float hAlign, MouseEvent onClick)
	{
		var button = new UIButton<LocalizedText>(Language.GetText(localizationKey), 1f, false) {
			Width = { Pixels = -10, Percent = 0.25f },
			Height = { Pixels = 40 },
			Top = { Pixels = -20 },
			VAlign = 1f,
			HAlign = hAlign,
			TooltipText = true,
			HoverSound = SoundID.MenuTick,
		};

		button.OnLeftClick += onClick;
		return button;
	}

	private static void SetButtonAltColorsToDisabled(UIButton<LocalizedText> button)
	{
		button.AltPanelColor = Color.Gray;
		button.AltHoverPanelColor = Color.Gray;
		button.AltHoverBorderColor = button.BorderColor;
	}

	private void CreatePanelContents()
	{
		var textBoxBackground = new UIPanel {
			Top = { Pixels = 15 },
			Width = { Pixels = 180 },
			Height = { Pixels = 30 },
			HAlign = 1f,
		}.WithPadding(0);
		uiPanel.Append(textBoxBackground);

		filterTextField = new UIFocusInputTextField(Language.GetText("tModLoader.ModConfigFilterOptions")) {
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		filterTextField.SetText("");
		filterTextField.OnTextChange += (_, _) => updateNeeded = true;
		filterTextField.OnRightClick += (_, _) => filterTextField.SetText("");
		textBoxBackground.Append(filterTextField);

		mainConfigList = new UIList {
			Width =  { Pixels = -25, Percent = 1f },
			Height = { Pixels = -55, Percent = 1f },
			VAlign = 1f,
			ListPadding = 5f,
		};
		uiPanel.Append(mainConfigList);

		scrollbar = new UIScrollbar {
			Height = { Pixels = -55, Percent = 1f },
			HAlign = 1f,
			VAlign = 1f,
		}.WithView(100f, 1000f);
		uiPanel.Append(scrollbar);
		mainConfigList.SetScrollbar(scrollbar);
	}

	private void CreateHeaderPanel()
	{
		// No localization should be needed here, this text should never be seen since it is usually replaced by the config and mod name
		headerTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModConfigModConfig"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uiElement.Append(headerTextPanel);
	}

	#endregion

	#region Back Button

	public UIState PreviousUIState { get; set; } // Unused interface property, manual logic in HandleBackButtonUsage instead

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
	}

	// Note that Escape key while in-game won't call this.
	public void HandleBackButtonUsage()
	{
		if (HasUnsavedChanges) {
			var confirmDialog = new UIConfirmDialog(
				false,
				Language.GetText("tModLoader.ModConfigBackUnsavedChanges"),
				(_, _) => {
                    HasUnsavedChanges = false;
                    HandleBackButtonUsage();
                }
			);

			Append(confirmDialog);
			return;
		}

		if (Main.gameMenu || !openedFromModder)
			SoundEngine.PlaySound(SoundID.MenuClose);

		if (Main.gameMenu) {
			Main.menuMode = Interface.modConfigListID;
			HandleOnCloseCallback();
		}
		else {
			if (openedFromModder)
				IngameFancyUI.Close();
			else
				Main.InGameUI.SetState(Interface.modConfigList);
		}
	}

	internal void HandleOnCloseCallback()
	{
		if (modderOnClose != null) {
			modderOnClose.Invoke();
			modderOnClose = null;
		}
	}

	#endregion

	internal void Unload()
	{
		mainConfigList?.Clear();
		mainConfigItems?.Clear();
		mod = null;
		sortedModConfigs = null;
		modConfig = null;
		pendingConfig = null;

		while (configPanelStack.Count > 1)
			uiElement.RemoveChild(configPanelStack.Pop());
	}

	// Refreshes the UI to refresh recent changes such as Save/Discard/Restore Defaults
	private void DoMenuModeState(bool preserveNotificationMessage = false)
	{
		this.preserveNotificationMessage = preserveNotificationMessage;
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
		if (!HasUnsavedChanges)
			return;

		var result = modConfig.SaveChanges(pendingConfig, status: SetMessage, silent: false);
		if (result == ConfigSaveResult.Success) // Don't clear out pending changes for needs reload or sent to server
		{
			DoMenuModeState(preserveNotificationMessage: true);
			HasUnsavedChanges = false;
		}
	}

	private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement)
	{
		if (HasDefaultValues)
			return;

		SoundEngine.PlaySound(SoundID.MenuOpen);
		pendingRevertDefaults = true;
		HasUnsavedChanges = true;
		SetMessage(Language.GetTextValue("tModLoader.ModConfigDefaultsRestored"), Color.Green);
		DoMenuModeState(preserveNotificationMessage: true);
	}

	private void RevertConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		if (!HasUnsavedChanges)
			return;

		SoundEngine.PlaySound(SoundID.MenuClose);
		DiscardChanges();
	}

	private void DiscardChanges()
	{
		SetMessage(Language.GetTextValue("tModLoader.ModConfigChangesReverted"), Color.Green);
		DoMenuModeState(preserveNotificationMessage: true);
		HasUnsavedChanges = false;
	}

	private bool pendingChanges;
	private bool pendingChangesUIUpdate;

	public void SetPendingChanges(bool changes = true)
	{
		HasUnsavedChanges |= changes;

		pendingChangesUIUpdate |= changes;
		pendingChanges |= changes;
	}

	public void SetMessage(string text, Color color)
	{
		// TODO: set message
		/*
		message.TextScale = 1f;
		message.SetText(Language.GetText("tModLoader.ModConfigNotification") + text);
		float width = FontAssets.MouseText.Value.MeasureString(text).X;
		if (width > 400) {
			message.TextScale = 400 / width;
			message.Recalculate();
		}
		message.TextColor = color;
		//*/
	}

	private bool netUpdate;

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (scrollToOption != null) {
			bool header = false;
			if (scrollToOption.StartsWith("Header:")) {
				scrollToOption = scrollToOption.Split("Header:", StringSplitOptions.RemoveEmptyEntries)[0];
				header = true;
			}
			// Potential future support: ModConfigShowcaseDataTypes@SomeClassA/Header:enabled, ModConfigShowcaseDataTypes@SomeList/3, ModConfigShowcaseMisc@collapsedList
			var desiredElement = mainConfigList._items.Find(x => {
				if (x is UISortableElement sortableElement && sortableElement.Children.FirstOrDefault() is ConfigElement configElement && configElement.MemberInfo.Name == scrollToOption) {
					if (configElement is ObjectElement objectElement && objectElement.separatePagePanel != null) {
						SwitchToSubConfig(objectElement.separatePagePanel);
						return true;
					}
					configElement.Flashing = true;
					return true;
				}
				return false;
			});

			if (header) {
				int index = mainConfigList._items.IndexOf(desiredElement);
				for (int i = index - 1; i >= 0; i--) {
					if (mainConfigList._items[i] is UISortableElement sortableElement && sortableElement.Children.FirstOrDefault() is HeaderElement headerElement) {
						desiredElement = sortableElement;
						break;
					}
				}
			}
			mainConfigList.Goto(delegate (UIElement element) {
				return element == desiredElement;
			}, center: centerScrolledOption);

			scrollToOption = null;
			centerScrolledOption = false;
		}

		UpdateCount++;

		// TODO: keep the buttons always added
		if (pendingChangesUIUpdate) {
			uiElement.Append(saveConfigButton);
			uiElement.Append(revertConfigButton);
			pendingChangesUIUpdate = false;
		}

		if (netUpdate) {
			DoMenuModeState();
			netUpdate = false;
		}

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

	public static string Tooltip { get; set; }

	public override void Draw(SpriteBatch spriteBatch)
	{
		Tooltip = null;

		base.Draw(spriteBatch);

		if (!string.IsNullOrEmpty(Tooltip)) {
			UICommon.TooltipMouseText(Tooltip);
		}

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}

	// do we need 2 copies? We can discard changes by reloading.
	// We can save pending changes by saving file then loading/reloading mods.
	// when we get new server configs from server...replace, don't save?
	// reload manually, reload fresh server config?
	// need some CopyTo method to preserve references....hmmm
	internal void SetMod(Mod mod, ModConfig config = null, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true)
	{
		HasUnsavedChanges = false;

		this.mod = mod;
		this.openedFromModder = openedFromModder;
		this.modderOnClose = onClose;
		this.scrollToOption = scrollToOption;
		this.centerScrolledOption = centerScrolledOption;
		if (ConfigManager.Configs.ContainsKey(mod)) {
			sortedModConfigs = ConfigManager.Configs[mod].OrderBy(x => x.DisplayName.Value).ToList();
			modConfig = sortedModConfigs[0];
			if (config != null) {
				modConfig = ConfigManager.Configs[mod].First(x => x == config);
				// TODO, decide which configs to show in game: modConfigs = ConfigManager.Configs[mod].Where(x => x.Mode == ConfigScope.ClientSide).ToList();
			}
			//modConfigClone = modConfig.Clone();

			// if in game, maybe have all configs open

		}
		else {
			throw new Exception($"There are no ModConfig for {mod.DisplayNameClean}, how did this happen?");
		}
	}

	private static bool pendingRevertDefaults;

	public override void OnActivate()
	{
		// TODO: temporary, for development
		RemoveAllChildren();
		OnInitialize();

		Interface.modConfigList.ModToSelectOnOpen = mod;
		filterTextField.SetText("");

		updateNeeded = false;

		if (!preserveNotificationMessage)
			SetMessage("", Color.White);
		preserveNotificationMessage = false;

		string configDisplayName = modConfig.DisplayName.Value;

		headerTextPanel.SetText(string.IsNullOrEmpty(configDisplayName) ? modConfig.Mod.DisplayName : modConfig.Mod.DisplayName + " - " + configDisplayName);
		pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
		pendingChanges = pendingRevertDefaults;

		if (pendingRevertDefaults) {
			pendingRevertDefaults = false;
			ConfigManager.Reset(pendingConfig);
			pendingChangesUIUpdate = true;
		}

		// TODO: fix sub configs in the future
		//uiElement.RemoveChild(configPanelStack.Peek());
		//uiElement.Append(uiPanel);
		mainConfigItems.Clear();
		mainConfigList.Clear();
		configPanelStack.Clear();
		configPanelStack.Push(uiPanel);
		subPageStack.Clear();

		//currentConfigList = mainConfigList;
		int top = 0;
		// load all mod config options into UIList
		// TODO: Inheritance with ModConfig? DeclaredOnly?

		uiPanel.BackgroundColor = UICommon.MainPanelBackground;

		var backgroundColorAttribute = (BackgroundColorAttribute)Attribute.GetCustomAttribute(pendingConfig.GetType(), typeof(BackgroundColorAttribute));

		if (backgroundColorAttribute != null) {
			uiPanel.BackgroundColor = backgroundColorAttribute.Color;
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
		else if (type == typeof(BuffDefinition)) {
			e = new BuffDefinitionElement();
		}
		else if (type == typeof(TileDefinition)) {
			e = new TileDefinitionElement();
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
		else if (type == typeof(long)) {
			e = new LongElement();
		}
		else if (type == typeof(ulong)) {
			e = new ULongElement();
		}
		else if (type.IsEnum) {
			if (list != null)
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
			else {
				SliderAttribute sliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderAttribute>(memberInfo, item, list);
				bool useNewElements = (Interface.modConfig == null || Interface.modConfig.mod.TModLoaderVersion.MajorMinor() >= new Version(2025, 9)) && sliderAttribute == null;
				if (useNewElements)
					e = new EnumElement2();
				else
					e = new EnumElement();
			}
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
		else if (type == typeof(object)) {
			e = new UIText($"{memberInfo.Name} can't be of the Type Object.");
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
		uIPanel.CopyStyle(Interface.modConfig.uiPanel);
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
			Interface.modConfig.uiElement.RemoveChild(uIPanel);
			Interface.modConfig.configPanelStack.Pop();
			Interface.modConfig.uiElement.Append(Interface.modConfig.configPanelStack.Peek());
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

			//if (hasToString)
			//	_TextDisplayFunction = () => index + 1 + ": " + (array[index]?.ToString() ?? "null");

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
		Interface.modConfig.uiElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
		Interface.modConfig.uiElement.Append(separateListPanel);
		Interface.modConfig.configPanelStack.Push(separateListPanel);
	}

	internal void BlockInput(UIElement dialog)
	{
		blockInput = new BlockInputElement(mainConfigList);
		blockInput.OnLeftMouseDown += UnblockInput;
		UIElement innerList = mainConfigList.Children.First();
		innerList.Append(blockInput);

		// Append to UIList.UIInnerList, this is necessary so it moves when scrolled
		innerList.Append(activeDialog = dialog);
	}

	internal void UnblockInput(UIMouseEvent evt, UIElement listeningElement)
	{
		blockInput?.Remove();
		activeDialog?.Remove();
	}
}

internal class BlockInputElement : UIElement
{
	private UIElement elementToBlock;

	public BlockInputElement(UIElement elementToBlock)
	{
		Width.Set(0, 1);
		Height.Set(0, 1);

		this.elementToBlock = elementToBlock;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		var drawArea = elementToBlock.GetDimensions().ToRectangle();
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawArea, Color.Black * 0.5f);
	}
}