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
	internal class UIModConfigFloatItem : UIConfigRangeItem
	{
		private Func<float> _GetValue;
		private Action<float> _SetValue;
		float min = 0;
		float max = 1;
		float increment = 0.01f;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (increment) / (max - min);

		public UIModConfigFloatItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<float> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (float value) => DefaultSetValue(value);

			if(array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (float value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValue();
			}

			drawTicks = (DrawTicksAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(DrawTicksAttribute)) != null;
			RangeAttribute rangeAttribute = (RangeAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(RangeAttribute));
			IncrementAttribute incrementAttribute = (IncrementAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(IncrementAttribute));
			if (rangeAttribute != null && rangeAttribute.min is float && rangeAttribute.max is float)
			{
				max = (float)rangeAttribute.max;
				min = (float)rangeAttribute.min;
			}
			if (incrementAttribute != null && rangeAttribute.min is float)
			{
				increment = (float)incrementAttribute.increment;
			}
			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		void DefaultSetValue(float value)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Utils.Clamp(value, min, max));
			Interface.modConfig.SetPendingChanges();
		}

		float DefaultGetValue()
		{
			return (float)memberInfo.GetValue(item);
		}

		float DefaultGetProportion()
		{
			return (_GetValue() - min) / (max - min);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((float)Math.Round((proportion * (max - min) + min) * (1 / increment)) * increment);
		}
	}
}
