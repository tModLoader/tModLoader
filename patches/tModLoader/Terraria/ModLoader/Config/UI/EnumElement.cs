using System;
using System.Collections;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	class EnumElement : RangeElement
	{
		private Func<object> _GetValue;
		private Func<object> _GetValueString;
		private Func<int> _GetIndex;
		private Action<int> _SetValue;
		int max;
		string[] valueStrings;

		public override int NumberTicks => valueStrings.Length;
		public override float TickIncrement => 1f / (valueStrings.Length - 1);

		protected override float Proportion {
			get => _GetIndex() / (float)(max - 1);
			set => _SetValue((int)(Math.Round(value * (max - 1))));
		}

		public override void OnBind() {
			base.OnBind();
			valueStrings = Enum.GetNames(memberInfo.Type);
			max = valueStrings.Length;

			//valueEnums = Enum.GetValues(variable.Type);

			TextDisplayFunction = () => memberInfo.Name + ": " + _GetValueString();
			_GetValue = () => DefaultGetValue();
			_GetValueString = () => DefaultGetStringValue();
			_GetIndex = () => DefaultGetIndex();
			_SetValue = (int value) => DefaultSetValue(value);

			//if (array != null)
			//{
			//	_GetValue = () => array[index];
			//	_SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
			//	_TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
			//}

			if (labelAttribute != null)
			{
				TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValueString();
			}
		}

		void DefaultSetValue(int index)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Enum.GetValues(memberInfo.Type).GetValue(index));
			Interface.modConfig.SetPendingChanges();
		}

		object DefaultGetValue()
		{
			return memberInfo.GetValue(item);
		}
		int DefaultGetIndex()
		{
			return Array.IndexOf(Enum.GetValues(memberInfo.Type), _GetValue());
		}
		string DefaultGetStringValue()
		{
			return valueStrings[_GetIndex()];
		}
	}
}
