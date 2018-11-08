using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Config.UI
{
	class StringOptionElement : RangeElement
	{
		//private Func<string> _TextDisplayFunction;

		private Func<string> _GetValue;
		private Func<int> _GetIndex;
		private Action<int> _SetValue;
		string[] options;

		public override int NumberTicks => options.Length;
		public override float TickIncrement => 1f / (options.Length - 1);

		public StringOptionElement(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<string> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item, (IList)array)
		{
			OptionStringsAttribute optionsAttribute = ConfigManager.GetCustomAttribute<OptionStringsAttribute>(memberInfo, item, array);
			options = optionsAttribute.optionLabels;

			_TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			_GetValue = () => DefaultGetValue();
			_GetIndex = () => DefaultGetIndex();
			_SetValue = (int value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (int value) => { array[index] = options[value]; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}

			_GetProportion = () => DefaultGetProportion();
			_SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		void DefaultSetValue(int index)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, options[index]);
			Interface.modConfig.SetPendingChanges();
		}

		string DefaultGetValue()
		{
			return (string)memberInfo.GetValue(item);
		}

		int DefaultGetIndex()
		{
			return Array.IndexOf(options, _GetValue());
		}

		float DefaultGetProportion()
		{
			return _GetIndex() / (float)(options.Length - 1);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((int)(Math.Round(proportion * (options.Length - 1))));
		}
	}
}
