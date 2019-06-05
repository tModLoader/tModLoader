using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Config.UI
{
	public class FloatElement : RangeElement
	{
		public float min = 0;
		public float max = 1;
		public float increment = 0.01f;
		public int index;
		public new IList<float> array;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (increment) / (max - min);

		public FloatElement(PropertyFieldWrapper memberInfo, object item, IList<float> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			this.array = array;
			this.index = index;
			this._TextDisplayFunction = () => memberInfo.Name + ": " + GetValue();

			if (array != null)
			{
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + GetValue();
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
		}

		protected override float Proportion {
			get => (GetValue() - min) / (max - min);
			set => SetValue((float)Math.Round((value * (max - min) + min) * (1 / increment)) * increment);
		}

		protected virtual void SetValue(float value) {
			if (array != null) {
				array[index] = value;
				Interface.modConfig.SetPendingChanges();
				return;
			}
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Utils.Clamp(value, min, max));
			Interface.modConfig.SetPendingChanges();
		}

		protected virtual float GetValue()
		{
			if (array != null)
				return array[index];
			return (float)memberInfo.GetValue(item);
		}
	}
}
