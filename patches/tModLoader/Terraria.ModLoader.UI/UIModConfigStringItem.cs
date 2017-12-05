using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	class UIModConfigStringItem : UIConfigRangeItem
	{
		//private Func<string> _TextDisplayFunction;

		private Func<string> _GetValue;
		private Func<int> _GetIndex;
		private Action<int> _SetValue;
		string[] options;

		public override int NumberTicks => options.Length;
		public override float TickIncrement => 1f / (options.Length - 1);

		public UIModConfigStringItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<string> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item)
		{
			OptionStringsAttribute optionsAttribute = (OptionStringsAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(OptionStringsAttribute));
			options = optionsAttribute.optionLabels;

			_TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			_GetValue = () => DefaultGetValue();
			_GetIndex = () => DefaultGetIndex();
			_SetValue = (int value) => DefaultSetValue(value);

			if(array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (int value) => { array[index] = options[value]; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValue();
			}

			_GetProportion = () => DefaultGetProportion();
			_SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		void DefaultSetValue(int index)
		{
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
