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
	// Used for animations in elements like NPCDefinition
	public int UpdateCount { get; private set; }

	private UIElement uIElement;
	private UIPanel uIPanel;
	private UIList configList;
	private UIScrollbar uIScrollbar;
	private UIFocusInputTextField searchBar;
	private UITextPanel<string> configNamePanel;
	private UITextPanel<LocalizedText> subConfigBackButton;
	private UIButton<LocalizedText> saveConfigButton;
	private UIButton<LocalizedText> backButton;
	private UIButton<LocalizedText> revertConfigButton;
	private UIButton<LocalizedText> restoreDefaultsConfigButton;
	private UIPanel notificationModal;
	private UIMessageBox notificationModalText;
	private UIText notificationModalHeader;
	private UIImage modalInputBlocker;

	// TODO: use only one config and just load from disk instead
	// do we need 2 copies? We can discard changes by reloading.
	// We can save pending changes by saving file then loading/reloading mods.
	// when we get new server configs from server...replace, don't save?
	// reload manually, reload fresh server config?
	// need some CopyTo method to preserve references....hmmm
	private Mod mod;
	private ModConfig config;// Load time config from ConfigManager.Configs
	private ModConfig pendingConfig;// The clone of the config that is modified

	private Stack<ConfigElement> subPages = new();
	private bool hasUnsavedChanges = false;
	private bool openedFromModder = false;

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

		var headerTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigModConfig"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uIElement.Append(headerTextPanel);

		// TODO: clean this up
		subConfigBackButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = { Pixels = 75 },
			Height = { Pixels = 40 },
			Top = { Pixels = 5 },
		};
		uIPanel.Append(subConfigBackButton);// TODO: temporary
		// Don't append

		// TODO: fix name overflowing
		configNamePanel = new UITextPanel<string>("") {
			Width = { Pixels = -185 - 85, Percent = 1f },
			Height = { Pixels = 40 },
			Top = { Pixels = 5 },
			Left = { Pixels = 80 },
		};
		uIPanel.Append(configNamePanel);

		var textBoxBackground = new UIPanel {
			Width = { Pixels = 175 },
			Height = { Pixels = 30 },
			Top = { Pixels = 10 },
			HAlign = 1f,
		}.WithPadding(0f);
		uIPanel.Append(textBoxBackground);

		// TODO: localize properly
		searchBar = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterOptions")) {
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		searchBar.OnTextChange += (_, _) => UpdateConfigList();
		searchBar.OnRightClick += (_, _) => searchBar.SetText("");
		searchBar.SetText("");
		textBoxBackground.Append(searchBar);

		float listTop = 50;
		configList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -listTop, Percent = 1f },
			Top = { Pixels = listTop },
			ListPadding = 5f,
		};
		uIPanel.Append(configList);

		uIScrollbar = new UIScrollbar {
			Top = { Pixels = listTop },
			Height = { Pixels = -listTop, Percent = 1f },
			HAlign = 1f,
		};
		uIScrollbar.SetView(100f, 1000f);
		configList.SetScrollbar(uIScrollbar);
		uIPanel.Append(uIScrollbar);

		backButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigBack")) {
			Width = { Pixels = -10, Percent = 0.25f },
			Height = { Pixels = 40 },
			Top = { Pixels = -20 },
			HAlign = 0 / 3f,
			VAlign = 1f,
			AltPanelColor = Color.Red * 0.7f,
			AltHoverPanelColor = Color.Red,
			UseAltColours = () => hasUnsavedChanges,
			ClickSound = SoundID.MenuClose,
			HoverSound = SoundID.MenuTick,
		};
		backButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement)
		{

			if (Main.gameMenu)
			{
				Main.menuMode = openedFromModder ? MenuID.Title : Interface.modConfigListID;
			}
			else
			{
				if (openedFromModder)
					IngameFancyUI.Close();
				else
					Main.InGameUI.SetState(Interface.modConfigList);
			}
		};
		uIElement.Append(backButton);

		saveConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig")) {
			ClickSound = SoundID.MenuOpen,
			HoverSound = SoundID.MenuTick,
		};
		saveConfigButton.CopyStyle(backButton);
		saveConfigButton.HAlign = 1 / 3f;
		saveConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			pendingConfig.Save();
			CheckSaveButton();
		};
		// Don't append

		revertConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigRevertChanges")) {
			ClickSound = SoundID.MenuClose,
			HoverSound = SoundID.MenuTick,
		};
		revertConfigButton.CopyStyle(backButton);
		revertConfigButton.HAlign = 2 / 3f;
		revertConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			ConfigManager.RevertConfig(pendingConfig, config);
			CheckSaveButton();
		};
		// Don't append

		restoreDefaultsConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigRestoreDefaults")) {
			ClickSound = SoundID.MenuOpen,
			HoverSound = SoundID.MenuTick,
		};
		restoreDefaultsConfigButton.CopyStyle(backButton);
		restoreDefaultsConfigButton.HAlign = 3 / 3f;
		restoreDefaultsConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			ConfigManager.Reset(pendingConfig);// Reset to defaults
			CheckSaveButton();
		};
		uIElement.Append(restoreDefaultsConfigButton);

		notificationModal = new UIPanel {// TODO: fix vertical alignment
			Width = { Pixels = 500 },
			Height = { Pixels = 350 },
			HAlign = 0.5f,
			VAlign = 0.5f,
			BackgroundColor = UICommon.MainPanelBackground * (1 / 0.8f),
		};
		// Don't append

		notificationModalText = new UIMessageBox("") {
			Width = { Percent = 1f },
			Height = { Pixels = -50, Percent = 1f },
			HAlign = 0.5f,
			VAlign = 1f,
			TextOriginX = 0.5f,
			TextOriginY = 0.5f,
		};
		notificationModal.Append(notificationModalText);

		notificationModalHeader = new UIText("", 0.75f, large: true) {
			Top = { Pixels = 10 },
			HAlign = 0.5f,
		};
		notificationModal.Append(notificationModalHeader);

		var modalCloseButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
		};
		modalCloseButton.OnLeftClick += (_, _) => ClearMessage();
		notificationModal.Append(modalCloseButton);

		modalInputBlocker = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = new Color(0, 0, 0, 0),
			ScaleToFit = true,
		};
		modalInputBlocker.OnLeftClick += (_, _) => ClearMessage();
		// Don't append
	}

	public override void OnActivate()
	{
		pendingConfig = ConfigManager.GeneratePopulatedClone(config);
		searchBar.SetText("");
		uIScrollbar.ViewPosition = 0f;
		subPages.Clear();
		ClearMessage(sound: false);
		RefreshUI();
	}

	internal void Unload()
	{
		mod = null;
		config = null;
		pendingConfig = null;

		UpdateCount = 0;
		configList?.Clear();
		subPages?.Clear();
	}

	internal void SetMod(Mod mod, ModConfig config, bool openedFromModder = false)
	{
		this.openedFromModder = openedFromModder;
		this.mod = mod;
		this.config = config;
	}

	public void RefreshUI()
	{
		UpdateConfigList();
		CheckSaveButton();
		UpdateSeparatePage();
	}

	// Updates the main config list
	// TODO: rework
	public void UpdateConfigList()
	{
		configList.Clear();

		int top = 0;
		int order = 0;
		foreach (PropertyFieldWrapper variable in ConfigManager.GetDisplayedVariables(pendingConfig))
		{
			HandleHeader(configList, ref top, ref order, variable);
			var element = WrapIt(configList, ref top, variable, pendingConfig, order++).Item2;
			configList.Add(element);
		}

		Recalculate();

		/*

		uIPanel.BackgroundColor = UICommon.MainPanelBackground;

		var backgroundColorAttribute = (BackgroundColorAttribute)Attribute.GetCustomAttribute(pendingConfig.GetType(), typeof(BackgroundColorAttribute));

		if (backgroundColorAttribute != null) {
			uIPanel.BackgroundColor = backgroundColorAttribute.Color;
		}

		int order = 0;
		int top = 0;
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(pendingConfig)) {
			if (variable.IsProperty && variable.Name == "Mode")
				continue;

			if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
				continue;

			HandleHeader(configList, ref top, ref order, variable);

			WrapIt(configList, ref top, variable, pendingConfig, order++);
		}*/
	}

	// Checks if the config has been changed and updates the save and revert buttons
	public void CheckSaveButton()
	{
		// Compare JSON because otherwise reference types act weird
		string pendingJson = JsonConvert.SerializeObject(pendingConfig, ConfigManager.serializerSettings);
		string existingJson = JsonConvert.SerializeObject(config, ConfigManager.serializerSettings);
				hasUnsavedChanges = pendingJson != existingJson;

		saveConfigButton.Remove();
		revertConfigButton.Remove();
		if (hasUnsavedChanges) {
			uIElement.Append(saveConfigButton);
			uIElement.Append(revertConfigButton);
		}
	}

	// Updates the header panel, separate page back button, and separate page contents
	// TODO
	public void UpdateSeparatePage()
	{
		string configName = mod.DisplayName + " - " + config.DisplayName.Value;
		string subPagesText = string.Join(" > ", subPages.Reverse());
		configNamePanel.SetText(configName + subPagesText);
	}

	public void OpenSeparatePage(ObjectElement element)
	{
		subPages.Push(element);
		UpdateSeparatePage();
	}

	public void ClearMessage(bool sound = true)
	{
		SetMessage("", "");
		if (sound)
			SoundEngine.PlaySound(SoundID.MenuClose);
	}

	public void SetMessage(string text, string header, Color color = default, bool sendChatMessage = true)
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
		notificationModalText.Activate();
		notificationModalText.SetText(text);
		notificationModalHeader.SetText(header);
		notificationModalHeader.TextColor = color;

		if (sendChatMessage && !Main.gameMenu && Main.InGameUI.CurrentState != Interface.modConfig)
			Main.NewText($"[c/{color.Hex3()}:{header}] - {text}");
	}

	// Make scrolling outside of the main list still scroll the main list (if there is scrollbar hell then this helps a lot)
	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);

		if (!configList.ContainsPoint(Main.MouseScreen))
			uIScrollbar.ViewPosition -= evt.ScrollWheelValue;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		UpdateCount++;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (revertConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRevertChangesHover"));

		if (restoreDefaultsConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRestoreDefaultsHover"));

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
	}

	public static UIElement GetConfigElement(PropertyFieldWrapper memberInfo)
	{
		UIElement element = new UIText("TODO");

		// Custom UI
		var customUI = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomModConfigItemAttribute>(memberInfo);
		if (customUI != null) {
			Type customUIType = customUI.Type;

			if (typeof(ConfigElement).IsAssignableFrom(customUIType)) {
				ConstructorInfo ctor = customUIType.GetConstructor(Array.Empty<Type>());

				if (ctor != null) {
					object instance = ctor.Invoke(new object[0]);
					element = instance as UIElement;
				}
				else {
					element = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not have an empty constructor.");
				}
			}
			else {
				element = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not inherit from ConfigElement.");
			}
		}

		switch (memberInfo.Type) {
			case var _ when Attribute.IsDefined(memberInfo.MemberInfo, typeof(HeaderAttribute)):
				element = new HeaderElement("");// TODO
				break;
			default:
				break;
		}

		return element;
	}




	// TODO: rework onwards
	// TODO: rename to GetUIConfigElement
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

			return new Tuple<UIElement, UIElement>(container, e);
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

	internal static (UIPanel, UIList, UIFocusInputTextField) MakeSeparateListPanel(object item, object subitem, PropertyFieldWrapper memberInfo, IList array, int index, Func<string> AbridgedTextDisplayFunction)
	{
		UIPanel uIPanel = new UIPanel();
		uIPanel.CopyStyle(Interface.modConfig.uIPanel);
		uIPanel.BackgroundColor = UICommon.MainPanelBackground;

		BackgroundColorAttribute bca = ConfigManager.GetCustomAttributeFromMemberThenMemberType<BackgroundColorAttribute>(memberInfo, subitem, null);

		if (bca != null) {
			uIPanel.BackgroundColor = bca.Color;
		}

		// Copied from the OnInitialize method
		var textBoxBackground = new UIPanel {
			Width = { Pixels = 175 },
			Height = { Pixels = 30 },
			Top = { Pixels = 10 },
			HAlign = 1f,
		}.WithPadding(0f);
		uIPanel.Append(textBoxBackground);

		var filterTextField = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterOptions")) {// TODO: localize
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		filterTextField.OnTextChange += (a, b) => Interface.modConfig.UpdateConfigList();
		filterTextField.OnRightClick += (a, b) => filterTextField.SetText("");
		filterTextField.SetText("");
		textBoxBackground.Append(filterTextField);
		// End of Ctrl + V

		UIList separateList = new UIList();
		separateList.CopyStyle(Interface.modConfig.configList);
		uIPanel.Append(separateList);

		UIScrollbar uIScrollbar = new UIScrollbar();
		uIScrollbar.CopyStyle(Interface.modConfig.uIScrollbar);
		uIScrollbar.SetView(100f, 1000f);
		uIPanel.Append(uIScrollbar);
		separateList.SetScrollbar(uIScrollbar);

		string name = ConfigManager.GetLocalizedLabel(memberInfo);
		if (index != -1)
			name = name + " #" + (index + 1);
		//Interface.modConfig.subPageStack.Push(name);
		//UIPanel heading = new UIPanel();
		//UIText headingText = new UIText(name);

		//name = string.Join(" > ", Interface.modConfig.subPageStack.Reverse()); //.Aggregate((current, next) => current + "/" + next);

		UITextPanel<string> heading = new UITextPanel<string>(name) {
			Left = { Pixels = 65 },
			Height = { Pixels = 20 },
			HAlign = 0f,
		}; // TODO: ToString as well. Separate label?
		uIPanel.Append(heading);

		var back = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigBack")) {
			Width = { Pixels = 50 },
			Height = { Pixels = 20 },
			HAlign = 0f,
		};
		back.OnLeftClick += (a, c) => {
			Interface.modConfig.uIElement.RemoveChild(uIPanel);
			//Interface.modConfig.configPanelStack.Pop();
			//Interface.modConfig.uIElement.Append(Interface.modConfig.configPanelStack.Peek());
			Interface.modConfig.RefreshUI();

			// Stop header getting overlapped by the main panel
			Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configNamePanel);
			Interface.modConfig.uIElement.Append(Interface.modConfig.configNamePanel);
			//Interface.modConfig.configPanelStack.Peek().SetScrollbar(Interface.modConfig.uIScrollbar);
			//Interface.modConfig.currentConfigList = capturedCurrent;
		};
		back.WithFadedMouseOver();
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
		}
		else {
			//ignoreSeparatePage just to simplify ToString label--> had some issues.
			//WrapIt(separateList, ref top, memberInfo, item, 1, ignoreSeparatePage: true);
		}

		//Interface.modConfig.subPageStack.Pop();
		return (uIPanel, separateList, filterTextField);
	}

	internal static void HandleHeader(UIElement parent, ref int top, ref int order, PropertyFieldWrapper variable)
	{
		HeaderAttribute header = ConfigManager.GetLocalizedHeader(variable.MemberInfo);

		if (header != null) {
			var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
			WrapIt(parent, ref top, wrapper, header, order++);
		}
	}
}
