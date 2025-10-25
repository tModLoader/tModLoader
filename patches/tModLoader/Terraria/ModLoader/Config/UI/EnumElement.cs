using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal class EnumElement : RangeElement
{
	private Func<object> _getValue;
	private Func<object> _getValueString;
	private Func<int> _getIndex;
	private Action<int> _setValue;
	private int max;
	private string[] valueStrings;
	private string[] tooltips;

	public override int NumberTicks => valueStrings.Length;
	public override float TickIncrement => 1f / (valueStrings.Length - 1);

	protected override float Proportion {
		get => _getIndex() / (float)(max - 1);
		set => _setValue((int)(Math.Round(value * (max - 1))));
	}

	public override void OnBind()
	{
		base.OnBind();
		valueStrings = Enum.GetNames(MemberInfo.Type);
		tooltips = new string[valueStrings.Length];

		// Retrieve individual Enum member labels
		for (int i = 0; i < valueStrings.Length; i++) {
			var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
			if (enumFieldFieldInfo != null) {
				string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
				valueStrings[i] = name;

				string tooltip = ConfigManager.GetLocalizedTooltip(new PropertyFieldWrapper(enumFieldFieldInfo));
				tooltips[i] = tooltip;
			}
		}

		max = valueStrings.Length;

		//valueEnums = Enum.GetValues(variable.Type);

		TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
		_getValue = () => DefaultGetValue();
		_getValueString = () => DefaultGetStringValue();
		_getIndex = () => DefaultGetIndex();
		_setValue = (int value) => DefaultSetValue(value);

		/*
		if (array != null) {
			_GetValue = () => array[index];
			_SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
			_TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
		}
		*/

		if (Label != null) {
			TextDisplayFunction = () => Label + ": " + _getValueString();
		}
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

	public override void Draw(SpriteBatch spriteBatch) {
		base.Draw(spriteBatch);

		if (IngameOptions.inBar) {
			int index = _getIndex();
			UIModConfig.Tooltip = index != -1 ? tooltips[index] : Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
		}
	}
}
