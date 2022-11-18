using System;

namespace Terraria.ModLoader.Config.UI;

public class FloatElement : PrimitiveRangeElement<float>
{
	public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
	public override float TickIncrement => (Increment) / (Max - Min);

	protected override float Proportion {
		get => (GetValue() - Min) / (Max - Min);
		set => SetValue((float)Math.Round((value * (Max - Min) + Min) * (1 / Increment)) * Increment);
	}

	public FloatElement()
	{
		Min = 0;
		Max = 1;
		Increment = 0.01f;
	}
}
