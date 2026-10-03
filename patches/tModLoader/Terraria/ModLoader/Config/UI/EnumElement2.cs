using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

/// <summary>
/// Supports 3 modes of operation:
/// Normal: Options are presented in an expanded panel
/// DropdownAttribute: Options are presented in a dropdown, fading out the background. If more than 4 enum elements, the dropdown becomes a wider UIGrid
/// CycleAttribute: Options are cycled through on left/right click
/// </summary>
internal class EnumElement2 : ConfigElement
{
	private UIAutoScaleTextTextPanel<string> OptionChoice { get; set; }
	private List<UIAutoScaleTextTextPanel<string>> Options { get; set; }
	private UIPanel ChooserPanel { get; set; }
	private NestedUIGrid ChooserList { get; set; }
	private bool UpdateNeeded { get; set; }
	private bool SelectionExpanded { get; set; }
	private bool DropDown { get; set; }
	private bool Cycle { get; set; }

	private Func<object> _getValue;
	private Func<string> _getValueString;
	private Func<int> _getIndex;
	private Action<int> _setValue;
	private int max;
	private string[] valueStrings;
	private string[] tooltips;
	private int hoveredIndex = -2; // -2 is no hover, -1 is unknown enum value

	public override void OnBind()
	{
		base.OnBind();
		valueStrings = Enum.GetNames(MemberInfo.Type);
		max = valueStrings.Length;
		tooltips = new string[max];

		if (ConfigManager.GetCustomAttributeFromMemberThenMemberType<DropdownAttribute>(MemberInfo, Item, List) != null) {
			DropDown = true;
		}
		else if (ConfigManager.GetCustomAttributeFromMemberThenMemberType<CycleAttribute>(MemberInfo, Item, List) != null) {
			Cycle = true;
		}

		// Retrieve individual Enum member labels
		for (int i = 0; i < max; i++) {
			var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
			if (enumFieldFieldInfo != null) {
				string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
				valueStrings[i] = name;

				string tooltip = ConfigManager.GetLocalizedTooltip(new PropertyFieldWrapper(enumFieldFieldInfo));
				tooltips[i] = tooltip;
			}
		}

		_getValue = DefaultGetValue;
		_getValueString = DefaultGetStringValue;
		_getIndex = DefaultGetIndex;
		_setValue = DefaultSetValue;

		OptionChoice = new UIAutoScaleTextTextPanel<string>(_getValueString());
		OptionChoice.SetPadding(0);
		OptionChoice.Width.Set(120 + 24 + 12, 0f);
		OptionChoice.UseInnerDimensions = true;
		OptionChoice.PaddingLeft = Cycle ? 6 : 36;
		OptionChoice.PaddingRight = 6;
		OptionChoice.Height.Set(30, 0f);
		OptionChoice.Left.Set(-4, 0f);
		OptionChoice.HAlign = 1f;
		OptionChoice.OnLeftClick += (a, b) => {
			if (Cycle) {
				_setValue((_getIndex() + 1) % max);
				UpdateNeeded = true;
			}
			else if (!DropDown) {
				SelectionExpanded = !SelectionExpanded;
				UpdateNeeded = true;
			}
			else {
				ShowDropdown();
			}
		};
		OptionChoice.OnRightClick += (a, b) => {
			if (Cycle) {
				int index = _getIndex();
				_setValue(index == -1 ? max - 1 : (index - 1 + max) % max);
				UpdateNeeded = true;
			}
		};
		OptionChoice.OnUpdate += (a) => {
			if (a.IsMouseHovering)
				hoveredIndex = _getIndex();
		};
		Append(OptionChoice);

		if (!Cycle) {
			var dropdownIcon = new UIImage(UICommon.DropdownIconTexture); //24x24
			dropdownIcon.MarginLeft = -12;
			dropdownIcon.MarginTop = -12;
			dropdownIcon.MarginLeft = -36;
			dropdownIcon.MarginTop = 0;
			dropdownIcon.RemoveFloatingPointsFromDrawPosition = true;
			OptionChoice.Append(dropdownIcon);
		}

		if (!DropDown || max > 4) {
			ChooserPanel = new UIPanel() {
				Top = new(30, 0),
				Width = new(-8, 1),
				Left = new(4, 0),
				BackgroundColor = Color.CornflowerBlue,
				// Each is 30 tall, and 5 list padding. 12 panel padding top and bottom minus the final row list padding
				Height = new(19 + (int)Math.Ceiling(max / 4f) * 35, 0),
			};
		}
		else {
			int desiredWidth = 132;
			ChooserPanel = new UIPanel() {
				Width = new(desiredWidth, 0f),
				Height = new(max * 35 + 12 - 1, 0f),
				BackgroundColor = Color.CornflowerBlue,
			};
		}

		ChooserList = new NestedUIGrid() {
			Height = new(30, 1),
			Width = new(0, 1),
		};
		ChooserList.ManualSortMethod = (e) => { };
		ChooserPanel.Append(ChooserList);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (ChooserPanel.IsMouseHovering)
			UIModConfig.Tooltip = "";
		if (hoveredIndex != -2)
			UIModConfig.Tooltip = hoveredIndex != -1 ? tooltips[hoveredIndex] : Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
	}

	private void ShowDropdown()
	{
		CalculatedStyle anchorButtonDimensions = OptionChoice.GetDimensions();
		// The top value we want is actually the UISortableElement containing this element: OptionChoice->This->UISortableElement
		ChooserPanel.Top.Set(OptionChoice.Parent.Parent.Top.Pixels + anchorButtonDimensions.Height, 0f);
		ChooserPanel.Left.Set(-4, 0f);
		ChooserPanel.HAlign = 1f;
		ChooserPanel.SetPadding(6f);

		if (!DropDown || max > 4) {
			ChooserPanel.SetPadding(12f);
			ChooserPanel.Left.Set(12, 0f);
			ChooserPanel.Width.Set(-24, 1);
			ChooserPanel.HAlign = 0f;
		}

		Interface.modConfig.BlockInput(ChooserPanel);

		if (Options == null) {
			Options = CreateDefinitionOptionElementList();
			ChooserList.Clear();
			ChooserList.AddRange(Options);
		}
	}

	public override void Update(GameTime gameTime)
	{
		hoveredIndex = -2;
		base.Update(gameTime);

		if (!UpdateNeeded)
			return;

		UpdateNeeded = false;

		if (SelectionExpanded && Options == null) {
			Options = CreateDefinitionOptionElementList();
			ChooserList.Clear();
			ChooserList.AddRange(Options);
		}

		if (!SelectionExpanded) {
			ChooserPanel.MouseOut(new UIMouseEvent(ChooserPanel, new Vector2(Main.mouseX, Main.mouseY))); // Without this, this element will still think it is hovered when the height changes because the MouseOut logic travels up the parents, but it doesn't have a parent anymore.
			ChooserPanel.Remove();
		}
		else {
			Append(ChooserPanel);
		}

		float newHeight = SelectionExpanded ? 30 + ChooserPanel.Height.Pixels + 4 : 30;
		Height.Set(newHeight, 0f);

		if (Parent != null && Parent is UISortableElement) {
			Parent.Height.Pixels = newHeight;
		}

		OptionChoice.SetText(_getValueString());
	}

	private List<UIAutoScaleTextTextPanel<string>> CreateDefinitionOptionElementList()
	{
		var options = new List<UIAutoScaleTextTextPanel<string>>();

		for (int i = 0; i < max; i++) {
			int index = i;
			var optionElement = new UIAutoScaleTextTextPanel<string>(valueStrings[i]);
			optionElement.Width.Set(120, 0f);
			optionElement.Height.Set(30, 0f);
			optionElement.OnLeftClick += (a, b) => {
				_setValue(index);
				UpdateNeeded = true;
				if (!DropDown)
					SelectionExpanded = false;
				else
					Interface.modConfig.UnblockInput(a, b);
			};
			optionElement.OnUpdate += (a) => {
				if (a.IsMouseHovering)
					hoveredIndex = index;
			};
			options.Add(optionElement);
		}

		return options;
	}

	private void DefaultSetValue(int index)
	{
		if (!MemberInfo.CanWrite)
			return;

		MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
		Interface.modConfig.SetPendingChanges();
	}

	private object DefaultGetValue()
	{
		return MemberInfo.GetValue(Item);
	}

	private int DefaultGetIndex()
	{
		return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
	}

	private string DefaultGetStringValue()
	{
		int index = _getIndex();
		if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
			return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
		return valueStrings[index];
	}
}
