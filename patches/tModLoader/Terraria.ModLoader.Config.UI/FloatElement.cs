using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Config.UI
{
	public class FloatElement : PrimitiveRangeElement<float>
	{
		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (increment) / (max - min);
		protected override float Proportion {
			get => (GetValue() - min) / (max - min);
			set => SetValue((float)Math.Round((value * (max - min) + min) * (1 / increment)) * increment);
		}

		public FloatElement() {
			min = 0;
			max = 1;
			increment = 0.01f;
		}
	}
}
