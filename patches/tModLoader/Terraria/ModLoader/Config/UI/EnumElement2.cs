using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;

namespace Terraria.ModLoader.Config.UI;

internal class EnumElement2 : ConfigElement
{
	private UIAutoScaleTextTextPanel<string> OptionChoice { get; set; }
	private List<UIAutoScaleTextTextPanel<string>> Options { get; set; }
	private UIPanel ChooserPanel { get; set; }
	private NestedUIGrid ChooserList { get; set; }
	private bool UpdateNeeded { get; set; }
	private bool SelectionExpanded { get; set; }

	private Func<object> _getValue;
	private Func<string> _getValueString;
	private Func<int> _getIndex;
	private Action<int> _setValue;
	private string[] valueStrings;
	
	private bool Expand { get; set; } // EXPERIMENTS
	private bool DropDownAlt { get; set; } // EXPERIMENTS

	public override void OnBind()
	{
		base.OnBind();
		valueStrings = Enum.GetNames(MemberInfo.Type);

		var ExpandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
		var DrawTicksAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DrawTicksAttribute>(MemberInfo, Item, List);
		Expand = ExpandAttribute != null;
		DropDownAlt = DrawTicksAttribute != null;

		// Retrieve individual Enum member labels
		for (int i = 0; i < valueStrings.Length; i++) {
			var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
			if (enumFieldFieldInfo != null) {
				string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
				valueStrings[i] = name;
			}
		}

		_getValue = () => DefaultGetValue();
		_getValueString = () => DefaultGetStringValue();
		_getIndex = () => DefaultGetIndex();
		_setValue = (int value) => DefaultSetValue(value);

		

		OptionChoice = new UIAutoScaleTextTextPanel<string>(_getValueString());
		OptionChoice.SetPadding(0);
		OptionChoice.Width.Set(120 + 24 + 12, 0f);
		OptionChoice.UseInnerDimensions = true;
		//OptionChoice.MarginLeft = 24;
		OptionChoice.PaddingLeft = 36;
		OptionChoice.PaddingRight = 6;
		//OptionChoice.TextOriginX = 1f;
		OptionChoice.Height.Set(30, 0f);
		OptionChoice.Left.Set(-4, 0f);
		OptionChoice.HAlign = 1f;
		OptionChoice.OnLeftClick += (a, b) => {
			if (Expand) {
				SelectionExpanded = !SelectionExpanded;
				UpdateNeeded = true;
			}
			else {
				ShowDropdown();
			}
		};
		Append(OptionChoice);

		var dropdownIcon = new UIImage(UICommon.DropdownIconTexture); //24x24
		dropdownIcon.MarginLeft = -12;
		dropdownIcon.MarginTop = -12;
		dropdownIcon.MarginLeft = -36;
		dropdownIcon.MarginTop = 0;
		dropdownIcon.RemoveFloatingPointsFromDrawPosition = true;
		OptionChoice.Append(dropdownIcon);

		//var divider = new UIImage(UICommon.DividerTexture); // 8x4 texture
		//divider.Rotation = MathHelper.PiOver2;
		//divider.ImageScale = 3; 
		//divider.Left.Set(24, 0);
		//OptionChoice.Append(divider);

		if (Expand || DropDownAlt) {
			ChooserPanel = new UIPanel();
			ChooserPanel.Top.Set(30, 0);
			// Each is 30 tall, and 5 list padding. 12 panel padding top and bottom minus the final row list padding
			ChooserPanel.Height.Set(19 + (int)Math.Ceiling(valueStrings.Length / 4f) * 35, 0);
			ChooserPanel.Width.Set(0, 1);
			ChooserPanel.BackgroundColor = Color.CornflowerBlue;

			ChooserList = new NestedUIGrid();
			ChooserList.Top.Set(0, 0);
			ChooserList.Height.Set(0, 1);
			ChooserList.Width.Set(0, 1);
			ChooserPanel.Append(ChooserList);
		}
		else {
			int desiredWidth = 132;
			//ChooserPanel = new UIPanel();
			//ChooserPanel.Top.Set(30, 0);
			//// Each is 30 tall, and 5 list padding. 12 panel padding top and bottom minus the final row list padding
			//ChooserPanel.Height.Set(19 + (int)Math.Ceiling(valueStrings.Length / 4f) * 35, 0);
			//ChooserPanel.Width.Set(0, 1);
			//ChooserPanel.BackgroundColor = Color.CornflowerBlue;


			ChooserPanel = new UIPanel() {
				Width = new Terraria.UI.StyleDimension(desiredWidth, 0f),
				Height = new Terraria.UI.StyleDimension(valueStrings.Length * 35 + 12 - 1, 0f),
				BackgroundColor = Color.CornflowerBlue,
				//BorderColor = Color.Black
			};
			Terraria.UI.CalculatedStyle anchorButtonDimensions = OptionChoice.GetDimensions();
			//// button.top is what we want, but actually the parent...
			//_toggleModsDialog.Top.Set(OptionChoice.Parent.Parent.Top.Pixels + anchorButtonDimensions.Height, 0f);
			//_toggleModsDialog.Left.Set(-4, 0f);
			//_toggleModsDialog.HAlign = 1f;
			//_toggleModsDialog.SetPadding(6f);
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		base.DrawChildren(spriteBatch);

		//spriteBatch.Draw(UICommon.DividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - _modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
	}

	private void ShowDropdown()
	{
		//if (DropDownAlt) {
			Terraria.UI.CalculatedStyle anchorButtonDimensions = OptionChoice.GetDimensions();
			// button.top is what we want, but actually the parent...
			ChooserPanel.Top.Set(OptionChoice.Parent.Parent.Top.Pixels + anchorButtonDimensions.Height, 0f);
			ChooserPanel.Left.Set(-4, 0f);
			ChooserPanel.HAlign = 1f;
			ChooserPanel.SetPadding(6f);
		//}
		if (DropDownAlt) {
			ChooserPanel.SetPadding(12f);
			ChooserPanel.Left.Set(12, 0f);
			ChooserPanel.Width.Set(-24, 1);
			ChooserPanel.HAlign = 0f;
		}

		//int desiredWidth = 132;

		//var _toggleModsDialog = new UIPanel() {
		//	Width = new Terraria.UI.StyleDimension(desiredWidth, 0f),
		//	Height = new Terraria.UI.StyleDimension(valueStrings.Length * 35 + 12 - 1, 0f),
		//	//HAlign = .5f,
		//	//VAlign = .5f,
		//	BackgroundColor = new Color(63, 82, 151),
		//	BorderColor = Color.Black
		//};
		//Terraria.UI.CalculatedStyle anchorButtonDimensions = OptionChoice.GetDimensions();
		//// button.top is what we want, but actually the parent...
		//_toggleModsDialog.Top.Set(OptionChoice.Parent.Parent.Top.Pixels + anchorButtonDimensions.Height, 0f);
		//_toggleModsDialog.Left.Set(-4, 0f);
		//_toggleModsDialog.HAlign = 1f;
		//_toggleModsDialog.SetPadding(6f);


		Interface.modConfig.BlockInput(ChooserPanel);

		//int y = 0;
		//foreach (var value in valueStrings) {
		//	var option = new UITextPanel<string>(value);
		//	option.Top.Set(y, 0f);
		//	option.OnLeftClick += (a, b) => {
		//		OptionChoice.SetText(value);
		//		Interface.modConfig.UnblockInput(a, b);
		//	};
		//	_toggleModsDialog.Append(option);
		//	y += 40;
		//}
		if (DropDownAlt)
			ChooserList.Clear();

		int y = 0;
		for (int i = 0; i < valueStrings.Length; i++) {
			int index = i;
			var optionElement = new UIAutoScaleTextTextPanel<string>(valueStrings[i]);
			optionElement.Width.Set(120, 0f);
			optionElement.Height.Set(30, 0f);
			if(!DropDownAlt)
				optionElement.Top.Set(y, 0f);
			optionElement.OnLeftClick += (a, b) => {
				_setValue(index);
				UpdateNeeded = true;
				//SelectionExpanded = false;

				Interface.modConfig.UnblockInput(a, b);
			};
			if (!DropDownAlt)
				ChooserPanel.Append(optionElement);
			else {
				ChooserList.Add(optionElement);
			}
			y += 35;
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!UpdateNeeded)
			return;

		UpdateNeeded = false;

		if (SelectionExpanded && Options == null) {
			Options = CreateDefinitionOptionElementList();
			ChooserList.Clear();
			ChooserList.AddRange(Options);
		}

		if (!SelectionExpanded)
			ChooserPanel.Remove();
		else
			Append(ChooserPanel);

		float newHeight = SelectionExpanded ? 240 : 30;
		if (SelectionExpanded)
			newHeight = 30 + ChooserPanel.Height.Pixels;
		Height.Set(newHeight, 0f);

		if (Parent != null && Parent is UISortableElement) {
			Parent.Height.Pixels = newHeight;
		}

		OptionChoice.SetText(_getValueString());
	}

	private List<UIAutoScaleTextTextPanel<string>> CreateDefinitionOptionElementList()
	{
		var options = new List<UIAutoScaleTextTextPanel<string>>();

		for (int i = 0; i < valueStrings.Length; i++) {
			int index = i;
			var optionElement = new UIAutoScaleTextTextPanel<string>(valueStrings[i]);
			optionElement.Width.Set(120, 0f);
			optionElement.Height.Set(30, 0f);
			optionElement.OnLeftClick += (a, b) => {
				_setValue(index);
				UpdateNeeded = true;
				SelectionExpanded = false;
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
