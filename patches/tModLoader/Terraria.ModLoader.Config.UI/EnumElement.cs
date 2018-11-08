using System;

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

		public EnumElement(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage) : base(sliderIDInPage, memberInfo, item, null)
		{

			valueStrings = Enum.GetNames(memberInfo.Type);
			max = valueStrings.Length;

			//valueEnums = Enum.GetValues(variable.Type);

			_TextDisplayFunction = () => memberInfo.Name + ": " + _GetValueString();
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
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValueString();
			}

			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
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

		float DefaultGetProportion()
		{
			return _GetIndex() / (float)(max - 1);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((int)(Math.Round(proportion * (max - 1))));
		}
	}
}
