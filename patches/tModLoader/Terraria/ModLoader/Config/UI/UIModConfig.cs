using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.Config.UI;

public class UIModConfig : UIState, IHaveBackButtonCommand, IConfigUI
{
	public int UpdateCount { get; protected set; }
	public string ConfigElementTooltip { get; set; }

	public bool HasUnsavedChanges { get; protected set; }
	public bool HasDefaultValues { get; protected set; }

	protected Mod mod;
	protected ModConfig modConfig; // This is from ConfigManager.Configs
	protected ModConfig pendingConfig; // The clone we modify, so we can revert changes easily

	protected ConfigPage rootConfigPage;
	protected ConfigPage CurrentConfigPage => configPageStack.Peek();
	protected bool InSubPage => configPageStack.Count > 1;
	protected readonly Stack<ConfigPage> configPageStack = new();

	public class ConfigPage(object name)
	{
		public object Name = name;
		public readonly List<Tuple<UIElement, UIElement>> ConfigElements = new();
		public Color BackgroundColor = UICommon.MainPanelBackground;
	}

	// TODO: make the input blocking API public
	private BlockInputElement blockInput;
	protected UIElement activeDialog;

	protected bool openedFromModder = false;
	protected Action modderOnClose = null;
	protected internal string scrollToOption = null;
	protected internal bool centerScrolledOption = false;

	protected bool refreshQueued = false;

	protected UIElement uiElement;
	protected UIPanel uiPanel;
	protected UIMarqueeTextPanel headerTextPanel;

	protected UIButton backButton;
	protected UIButton saveConfigButton;
	protected UIButton revertConfigButton;
	protected UIButton restoreDefaultsConfigButton;

	protected UIElement listHeaderContainer;
	protected UIList configElementList;
	protected UIScrollbar scrollbar;
	// TODO: make the input fields public
	private UIFocusInputTextField filterTextField;
	protected UIImageFramed configSideIndicator;
	// These 2 use the same code as UIModConfigList
	protected UIButton modNamePanel;
	protected UIImageFramed smallModIcon;

	protected UIButton subPageBackButton;
	protected UIPanel subPageBreadcrumbPanel;
	protected UIMarqueeText subPageBreadcrumbText;

	protected Color InfoBadgeBackgroundColor => UICommon.DefaultUIBlueMouseOver * 0.5f;
	protected Color InfoBadgeBorderColor => UICommon.DefaultUIBlueMouseOver;

	#region UI Creation

	public override void OnInitialize()
	{
		CreateMainPanel();
		CreateButtons();
		CreatePanelContents();
		CreateHeaderPanel();
		CreateSubPageUI();
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
		var clickableState = UIButton.State.DefaultWithHover with {
			TooltipText = true,
		};

		var backButtonUnsavedChanges = clickableState with {
			PanelColor = Color.Red * 0.7f,
			HoverPanelColor = Color.Red,
			HoverText = Language.GetText("tModLoader.ModConfigBackUnsavedChanges"),
		};

		var disabledState = UIButton.State.Default with {
			TooltipText = true,
			PanelColor = Color.Gray,
		};

		backButton = CreateButton("tModLoader.ModConfigBack", 0, BackClick);
		backButton.DefaultState = clickableState;
		backButton.AlternateState = backButtonUnsavedChanges;
		backButton.UseAlternateState = () => HasUnsavedChanges;
		uiElement.Append(backButton);

		saveConfigButton = CreateButton("tModLoader.ModConfigSaveConfig", 1f / 3f, SaveConfig);
		saveConfigButton.DefaultState = disabledState;
		saveConfigButton.AlternateState = clickableState;
		saveConfigButton.UseAlternateState = () => HasUnsavedChanges;
		saveConfigButton.DefaultState.HoverText = Language.GetText("tModLoader.ModConfigSaveConfigNoUnsavedChanges");
		uiElement.Append(saveConfigButton);

		revertConfigButton = CreateButton("tModLoader.ModConfigRevertChanges", 2f / 3f, RevertConfig);
		revertConfigButton.DefaultState = disabledState;
		revertConfigButton.AlternateState = clickableState;
		revertConfigButton.UseAlternateState = () => HasUnsavedChanges;
		revertConfigButton.DefaultState.HoverText = Language.GetText("tModLoader.ModConfigRevertChangesNoUnsavedChanges");
		revertConfigButton.AlternateState.HoverText = Language.GetText("tModLoader.ModConfigRevertChangesTooltip");
		uiElement.Append(revertConfigButton);

		restoreDefaultsConfigButton = CreateButton("tModLoader.ModConfigRestoreDefaults", 1f, RestoreDefaults);
		restoreDefaultsConfigButton.DefaultState = disabledState;
		restoreDefaultsConfigButton.AlternateState = clickableState;
		restoreDefaultsConfigButton.UseAlternateState = () => !HasDefaultValues;
		restoreDefaultsConfigButton.DefaultState.HoverText = Language.GetText("tModLoader.ModConfigRestoreDefaultsAlreadyDefault");
		restoreDefaultsConfigButton.AlternateState.HoverText = Language.GetText("tModLoader.ModConfigRestoreDefaultsTooltip");
		uiElement.Append(restoreDefaultsConfigButton);
	}

	private static UIButton CreateButton(string localizationKey, float hAlign, MouseEvent onClick)
	{
		var button = new UIButton(Language.GetText(localizationKey), 1f, false) {
			MinWidth = { Pixels = -10, Percent = 0.25f },
			Height = { Pixels = 40 },
			Top = { Pixels = -20 },
			VAlign = 1f,
			HAlign = hAlign,
		};

		button.MaxWidth = button.MinWidth;
		button.OnLeftClick += onClick;

		return button;
	}

	private void CreatePanelContents()
	{
		listHeaderContainer = new UIElement {
			Width = { Percent = 1f },
			Height = { Pixels = 40 },
		};
		uiPanel.Append(listHeaderContainer);

		var textBoxBackground = new UIPanel {
			Width = { Pixels = 180 },
			Height = { Pixels = 30 },
			HAlign = 1f,
			VAlign = 0.5f,
		}.WithPadding(0);
		listHeaderContainer.Append(textBoxBackground);

		filterTextField = new UIFocusInputTextField(Language.GetText("tModLoader.ModConfigFilterOptions")) {
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		filterTextField.SetText("");
		filterTextField.OnTextChange += (_, _) => RefreshUI();
		filterTextField.OnRightClick += (_, _) => filterTextField.SetText("");
		textBoxBackground.Append(filterTextField);

		var configSideIndicatorPanel = new UIPanel {
			Width = { Pixels = 40 },
			Height = { Pixels = 40 },
			VAlign = 0.5f,
			BackgroundColor = InfoBadgeBackgroundColor,
			BorderColor = InfoBadgeBorderColor,
		};
		configSideIndicatorPanel.SetPadding(0);
		listHeaderContainer.Append(configSideIndicatorPanel);

		configSideIndicator = new UIImageFramed(Asset<Texture2D>.Empty, Rectangle.Empty) {
			HAlign = 0.5f,
			VAlign = 0.5f,
		};
		configSideIndicatorPanel.Append(configSideIndicator);

		configSideIndicatorPanel.OnDraw += delegate {
			if (configSideIndicatorPanel.IsMouseHovering) {
				string hoverText = Language.GetTextValue(pendingConfig.Mode == ConfigScope.ServerSide ? "tModLoader.ModConfigServerSide" : "tModLoader.ModConfigClientSide");
				UICommon.TooltipMouseText(hoverText);
			}
		};

		(modNamePanel, smallModIcon) = UIModConfigList.CreatePanel("", Mod.PlaceholderSmallModIcon, null);
		modNamePanel.MaxWidth.Set(330, 0);
		modNamePanel.MinWidth.Set(0, 0);
		modNamePanel.VAlign = 0.5f;
		modNamePanel.Left.Pixels = 50;
		modNamePanel.DefaultState = UIButton.State.Default with {
			PanelColor = InfoBadgeBackgroundColor,
			BorderColor = InfoBadgeBorderColor,
		};

		listHeaderContainer.Append(modNamePanel);

		configElementList = new UIList {
			Width =  { Percent = 1f },
			Height = { Pixels = -listHeaderContainer.Height.Pixels - 5, Percent = 1f },
			VAlign = 1f,
			ListPadding = 5f,
		};
		uiPanel.Append(configElementList);

		scrollbar = new UIScrollbar {
			Height = { Pixels = configElementList.Height.Pixels, Percent = 1f },
			HAlign = 1f,
			VAlign = 1f,
		}.WithView(100f, 1000f);
		uiPanel.Append(scrollbar);
		configElementList.SetScrollbar(scrollbar);
	}

	private void CreateHeaderPanel()
	{
		headerTextPanel = new UIMarqueeTextPanel("[Unknown Config Name]", 0.8f, true) {
			MaxWidth = { Percent = 0.95f},
			HAlign = 0.5f,
			Top = { Pixels = -46 }, // -35 is common for most UIs, but UIWorkshopHub uses -46 to fit more content
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uiElement.Append(headerTextPanel);
	}

	private void CreateSubPageUI()
	{
		subPageBackButton = new UIButton(Language.GetText("tModLoader.ModConfigBack"), 1f, false) {
			MinWidth = { Pixels = 100 },
			Height = { Pixels = 40 },
			HAlign = 1f,
			Top = { Pixels = listHeaderContainer.Height.Pixels + 5 },
		};

		subPageBackButton.MaxWidth = subPageBackButton.MinWidth;
		subPageBackButton.DefaultState = UIButton.State.DefaultWithHover;
		subPageBackButton.OnLeftClick += BackSubPage;

		subPageBreadcrumbPanel = new UIPanel {
			Height = { Pixels = 24 },
			Width = { Percent = 1f, Pixels = -(subPageBackButton.MaxWidth.Pixels + 10) },
			BackgroundColor = new Color(35, 40, 83),
			BorderColor = UICommon.DefaultUIBlueMouseOver * 0.5f,
		}.WithPadding(0);

		subPageBreadcrumbPanel.PaddingLeft = 10;
		subPageBreadcrumbPanel.PaddingRight = 10;
		subPageBreadcrumbPanel.Top.Pixels = subPageBackButton.Top.Pixels + (subPageBackButton.Height.Pixels - subPageBreadcrumbPanel.Height.Pixels) / 2f;

		var subPagePrompt = new UIText(Language.GetText("tModLoader.ModConfigSubPage"), 0.8f) {
			VAlign = 0.5f,
			TextColor = Color.Gray,
		};
		subPageBreadcrumbPanel.Append(subPagePrompt);

		float leftPadding = subPagePrompt.MinWidth.Pixels + 5;
		subPageBreadcrumbText = new UIMarqueeText("", 0.8f) {
			MaxWidth = { Percent = 1f, Pixels = -leftPadding },
			Left = { Pixels = leftPadding },
			VAlign = 0.5f,
			TextOriginX = 0f,
		};

		subPageBreadcrumbPanel.Append(subPageBreadcrumbText);
	}

	#endregion

	#region Back Button

	public UIState PreviousUIState { get; set; } // Unused interface property, manual logic in HandleBackButtonUsage instead

	private bool isConfirmDiscardChangsPopupOpen = false;

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
	}

	// Note that Escape key while in-game won't call this.
	public void HandleBackButtonUsage()
	{
		if (HasUnsavedChanges) {
			if (isConfirmDiscardChangsPopupOpen) {
				return;
			}

			var confirmDialog = new UIConfirmDialog(
				showYesDontShowAgainButton: false,
				Language.GetText("tModLoader.ModConfigBackUnsavedChangesPopup"),
				Language.GetText("tModLoader.ModConfigBackUnsavedChangesPopupSubText"),
				(_, _) => {
					HasUnsavedChanges = false;
					HandleBackButtonUsage();
				}
			) {
				OnClose = () => isConfirmDiscardChangsPopupOpen = false,
			};

			SoundEngine.PlaySound(SoundID.MenuOpen);
			Append(confirmDialog);
			isConfirmDiscardChangsPopupOpen = true;
			return;
		}

		if (Main.gameMenu || !openedFromModder)
			SoundEngine.PlaySound(SoundID.MenuClose);

		if (Main.gameMenu) {
			Interface.modConfigList.ModToSelectOnOpen = mod;
			Main.menuMode = Interface.modConfigListID;
			HandleOnCloseCallback();
		}
		else {
			if (openedFromModder) {
				// HandleOnCloseCallback() called within because pressing escape in game calls IngameFancyUI.Close()
				IngameFancyUI.Close();
			}
			else {
				Interface.modConfigList.ModToSelectOnOpen = mod;
				Main.InGameUI.SetState(Interface.modConfigList);
			}
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

	#region Button Actions

	private void SaveConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		if (!HasUnsavedChanges)
			return;

		modConfig.SaveChanges(pendingConfig, status: SetMessage, silent: false);
		OnSaveRevertRestore(); // Temporary, read the comment inside OnSaveRevertRestore
		RefreshUI();
	}

	private void RevertConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		if (!HasUnsavedChanges)
			return;

		SoundEngine.PlaySound(SoundID.MenuOpen);
		SetMessage(Language.GetTextValue("tModLoader.ModConfigChangesReverted"), Color.Green);
		pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
		OnSaveRevertRestore(); // Temporary, read the comment inside OnSaveRevertRestore
		RefreshUI();
	}

	private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement)
	{
		if (HasDefaultValues)
			return;

		SoundEngine.PlaySound(SoundID.MenuOpen);
		SetMessage(Language.GetTextValue("tModLoader.ModConfigDefaultsRestored"), Color.Green);
		ConfigManager.Reset(pendingConfig);
		OnSaveRevertRestore(); // Temporary, read the comment inside OnSaveRevertRestore
		RefreshUI();
	}

	private void BackSubPage(UIMouseEvent evt, UIElement listeningElement)
	{
		if (configPageStack.Count <= 1) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuClose);
		configPageStack.Pop();
		RefreshUI();
	}

	#endregion

	#region UI Updating

	internal void Unload()
	{
		ResetUI();

		mod = null;
		modConfig = null;
		pendingConfig = null;
	}

	private void ResetUI()
	{
		rootConfigPage = null;
		configPageStack?.Clear();
		configElementList?.Clear();
		filterTextField?.SetText("");
		headerTextPanel?.ResetScroll();
		modNamePanel?.ResetScroll();
		subPageBreadcrumbText?.ResetScroll();
		UnblockInput(null, null);
		UpdateCount = 0;

		if (scrollbar is not null)
			scrollbar.ViewPosition = 0f;
	}

	private void OnSaveRevertRestore()
	{
		// This is a leftover from the previous config UI, since properly refreshing the config elements requires a lot of code changes and will probably break mods.
		// Currently, it is easier and less bug prone to just recreate the config UI.
		// This (unfortunately) preserves the legacy behaviour of configs going back to the main config page on save/revert/restore.
		configPageStack.Clear();
		rootConfigPage = new ConfigPage(modConfig.DisplayName);
		SetupConfigPage(rootConfigPage, pendingConfig, null);
		configPageStack.Push(rootConfigPage);
	}

	public void OnConfigModified()
	{
		RefreshUI();
	}

	// Exists to avoid breaking mods that depended on this method signature, will be removed in the future
	public void SetPendingChanges(bool changes = true)
	{
		if (changes)
			OnConfigModified();
	}

	private void RefreshUI(bool delayRefresh = true)
	{
		if (delayRefresh) {
			refreshQueued = true;
			return;
		}

		refreshQueued = false;

		RefreshSubPageUI();
		RefreshSaveAndRestoreConditions();
		RefreshConfigElementList();
		RefreshScrollbar();
		uiPanel.BackgroundColor = CurrentConfigPage.BackgroundColor;

		Recalculate();
	}

	private void RefreshSubPageUI()
	{
		subPageBackButton.Remove();
		subPageBreadcrumbPanel.Remove();

		configElementList.Height.Pixels = -listHeaderContainer.Height.Pixels - 5;

		if (InSubPage) {
			uiPanel.Append(subPageBackButton);
			uiPanel.Append(subPageBreadcrumbPanel);

			subPageBackButton.OnActivate(); // Refresh colors
			subPageBreadcrumbText.SetText(string.Join($" [c/{Color.DarkGray.Hex3()}:>] ", configPageStack.Reverse().Skip(1).Select(p => p.Name)));
			subPageBreadcrumbText.ResetScroll();
			subPageBreadcrumbText.MarqueeText.SetScrollDelay(60);

			configElementList.Height.Pixels -= subPageBackButton.Height.Pixels + 10;
		}

		scrollbar.Height.Pixels = configElementList.Height.Pixels;
	}

	private void RefreshSaveAndRestoreConditions()
	{
		HasUnsavedChanges = pendingConfig.HasChanges(modConfig);
		HasDefaultValues = pendingConfig.HasDefaultValues();
	}

	private void RefreshConfigElementList()
	{
		// TODO: allow filtering to look inside of subpages
		// TODO: should a SearchFunction should be used instead to allow searching labels, tooltips, children, and "related phrases" that a modder can specify?
		configElementList.Clear();
		configElementList.AddRange(CurrentConfigPage.ConfigElements.Where(item => {
			if (item.Item2 is ConfigElement configElement) {
				return configElement.TextDisplayFunction().Contains(filterTextField.CurrentString, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}).Select(x => x.Item1));
	}

	private void RefreshScrollbar()
	{
		configElementList.Recalculate();
		configElementList.Width.Pixels = 0;
		scrollbar.Remove();

		if (scrollbar.CanScroll) {
			configElementList.Width.Pixels -= 25;
			uiPanel.Append(scrollbar);
		}

		configElementList.Recalculate();
	}

	public void PushConfigPage(ConfigPage configPage)
	{
		configPageStack.Push(configPage);
		RefreshUI();
	}

	public void SetMessage(string text, Color color)
	{
		var notification = new ConfigNotification(text, color);
		InGameNotificationsTracker.AddMenuNotification(notification);
	}

	public void SetMod(Mod mod, ModConfig config, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true)
	{
		this.mod = mod;
		this.modConfig = config;
		this.pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
		this.openedFromModder = openedFromModder;
		this.modderOnClose = onClose;
		this.scrollToOption = scrollToOption;
		this.centerScrolledOption = centerScrolledOption;
	}

	public override void Update(GameTime gameTime)
	{
		if (refreshQueued)
			RefreshUI(delayRefresh: false);

		base.Update(gameTime);

		UpdateCount++;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		ConfigElementTooltip = null;

		base.Draw(spriteBatch);

		if (!string.IsNullOrEmpty(ConfigElementTooltip)) {
			UICommon.TooltipMouseText(ConfigElementTooltip);
		}

		// Draw the "Menu notifications" if we are in game, since they aren't drawn otherwise
		if (!Main.gameMenu) {
			InGameNotificationsTracker.DrawInMenu(spriteBatch);
		}

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}

	public override void OnActivate()
	{
		ResetUI();

		// From UIModConfigList
		var configSideTexture = UICommon.ConfigSideIndicatorTexture;
		var configSideFrame = configSideTexture.Frame(2, 1, pendingConfig.Mode == ConfigScope.ServerSide ? 1 : 0, 0, -2);
		configSideIndicator.SetImage(configSideTexture, configSideFrame);
		configSideIndicator.Recalculate();

		headerTextPanel.SetText(modConfig.DisplayName);
		headerTextPanel.Recalculate();
		headerTextPanel.ResetScroll();
		headerTextPanel.MarqueeText.SetScrollDelay(60);

		modNamePanel.SetText(modConfig.Mod.DisplayName);
		modNamePanel.Recalculate();
		modNamePanel.ResetScroll();

		var iconTexture = modConfig.Mod.SmallModIcon ?? Mod.PlaceholderSmallModIcon;
		smallModIcon.SetImage(iconTexture, Utils.CornerRectangle(Vector2.Zero, iconTexture.Size()));

		configPageStack.Clear();
		rootConfigPage = new ConfigPage(modConfig.DisplayName);
		SetupConfigPage(rootConfigPage, pendingConfig, null);
		PushConfigPage(rootConfigPage);

		RefreshUI(delayRefresh: false);

		if (scrollToOption != null) {
			ScrollTo(scrollToOption, centerScrolledOption);
			scrollToOption = null;
			centerScrolledOption = false;
		}

		if (!Main.gameMenu) {
			InGameNotificationsTracker.ClearMenuNotifications();
		}
	}

	public void ScrollTo(string scrollToOption, bool centerScrolledOption)
	{
		bool header = false;
		if (scrollToOption.StartsWith("Header:")) {
			scrollToOption = scrollToOption.Split("Header:", StringSplitOptions.RemoveEmptyEntries)[0];
			header = true;
		}
		// Potential future support: ModConfigShowcaseDataTypes@SomeClassA/Header:enabled, ModConfigShowcaseDataTypes@SomeList/3, ModConfigShowcaseMisc@collapsedList
		var desiredElement = configElementList._items.Find(x => {
			if (x is UISortableElement sortableElement && sortableElement.Children.FirstOrDefault() is ConfigElement configElement && configElement.MemberInfo.Name == scrollToOption) {
				if (configElement is ObjectElement objectElement && objectElement.separateConfigPage != null) {
					PushConfigPage(objectElement.separateConfigPage);
					return true;
				}
				configElement.Flashing = true;
				return true;
			}
			return false;
		});

		if (header) {
			int index = configElementList._items.IndexOf(desiredElement);
			for (int i = index - 1; i >= 0; i--) {
				if (configElementList._items[i] is UISortableElement sortableElement && sortableElement.Children.FirstOrDefault() is HeaderElement headerElement) {
					desiredElement = sortableElement;
					break;
				}
			}
		}
		configElementList.Goto(delegate (UIElement element) {
			return element == desiredElement;
		}, center: centerScrolledOption);
	}

	#endregion

	#region ConfigElement Handling

	internal static void SetupConfigPage(ConfigPage configPage, object config, PropertyFieldWrapper memberInfo)
	{
		BackgroundColorAttribute backgroundColorAttribute = memberInfo != null
			? ConfigManager.GetCustomAttributeFromMemberThenMemberType<BackgroundColorAttribute>(memberInfo, config, null)
			: (BackgroundColorAttribute)Attribute.GetCustomAttribute(config.GetType(), typeof(BackgroundColorAttribute));

		if (backgroundColorAttribute is not null) {
			configPage.BackgroundColor = backgroundColorAttribute.Color;
		}

		int top = 0;
		int order = 0;
		// ReSharper disable once LoopCanBePartlyConvertedToQuery
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(config)) {
			if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
				continue;

			var header = HandleAndGetHeader(null, ref top, ref order, variable);
			if (header is not null) {
				configPage.ConfigElements.Add(header);
			}

			configPage.ConfigElements.Add(WrapIt(null, ref top, variable, config, order++));
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

		#region Big if statement

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
		else if (type == typeof(WallDefinition)) {
			e = new WallDefinitionElement();
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
				bool useNewElements = true;//(Interface.modConfig == null || Interface.modConfig.mod.TModLoaderVersion.MajorMinor() >= new Version(2025, 9)) && sliderAttribute == null;
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

		#endregion

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
			else if (parent is not null) {
				// Only Vector2 and Color use this I think, but modders can use the non-UIList approach for custom UI and layout.
				container.Top.Pixels = top;
				container.Width.Pixels = -20;
				container.Left.Pixels = 20;
				top += elementHeight + 4;
				parent.Append(container);
				parent.Height.Set(top, 0);
			}

			var tuple = new Tuple<UIElement, UIElement>(container, e);

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

	// For API compat
	public static void HandleHeader(UIElement parent, ref int top, ref int order, PropertyFieldWrapper variable)
	{
		HandleAndGetHeader(parent, ref top, ref order, variable);
	}

	public static Tuple<UIElement, UIElement> HandleAndGetHeader(UIElement parent, ref int top, ref int order, PropertyFieldWrapper variable)
	{
		HeaderAttribute header = ConfigManager.GetLocalizedHeader(variable.MemberInfo);

		if (header != null) {
			var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
			return WrapIt(parent, ref top, wrapper, header, order++);
		}

		return null;
	}

	#endregion

	#region Input Blocking

	internal void BlockInput(UIElement dialog)
	{
		blockInput = new BlockInputElement(configElementList);
		blockInput.OnLeftMouseDown += UnblockInput;
		UIElement innerList = configElementList.Children.First();
		innerList.Append(blockInput);

		// Append to UIList.UIInnerList, this is necessary so it moves when scrolled
		innerList.Append(activeDialog = dialog);
	}

	internal void UnblockInput(UIMouseEvent evt, UIElement listeningElement)
	{
		blockInput?.Remove();
		activeDialog?.Remove();
	}

	#endregion
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
