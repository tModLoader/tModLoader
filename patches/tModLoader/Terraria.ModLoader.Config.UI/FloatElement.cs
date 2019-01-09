using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Config.UI
{
	internal class FloatElement : RangeElement
	{
		private Func<float> _GetValue;
		private Action<float> _SetValue;
		internal float min = 0;
		internal float max = 1;
		internal float increment = 0.01f;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (increment) / (max - min);

		public FloatElement(PropertyFieldWrapper memberInfo, object item, IList<float> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (float value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (float value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}

			if (rangeAttribute != null && rangeAttribute.min is float && rangeAttribute.max is float)
			{
				max = (float)rangeAttribute.max;
				min = (float)rangeAttribute.min;
			}
			if (incrementAttribute != null && incrementAttribute.increment is float)
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
