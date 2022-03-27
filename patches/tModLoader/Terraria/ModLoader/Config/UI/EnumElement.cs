using System;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class EnumElement : RangeElement
	{
		private Func<object> _getValue;
		private Func<object> _getValueString;
		private Func<int> _getIndex;
		private Action<int> _setValue;
		private int max;
		private string[] valueStrings;

		public override int NumberTicks => valueStrings.Length;
		public override float TickIncrement => 1f / (valueStrings.Length - 1);

		protected override float Proportion {
			get => _getIndex() / (float)(max - 1);
			set => _setValue((int)(Math.Round(value * (max - 1))));
		}

		public override void OnBind() {
			base.OnBind();
			valueStrings = Enum.GetNames(MemberInfo.Type);
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

			if (LabelAttribute != null) {
				TextDisplayFunction = () => LabelAttribute.Label + ": " + _getValueString();
			}
		}

		private void DefaultSetValue(int index) {
			if (!MemberInfo.CanWrite)
				return;

			MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
			Interface.modConfig.SetPendingChanges();
		}

		private object DefaultGetValue() {
			return MemberInfo.GetValue(Item);
		}

		private int DefaultGetIndex() {
			return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
		}

		private string DefaultGetStringValue() {
			return valueStrings[_getIndex()];
		}
	}
}
