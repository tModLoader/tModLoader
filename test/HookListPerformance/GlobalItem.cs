using System;

namespace HookListPerformance
{
	class GlobalItem
	{
		public int index;
		public float rng;

		public float DoEffect(ref float input) => input *= MathF.Sin(input * rng);

		public virtual float DoEffectVirtual(ref float input) => DoEffect(ref input);
	}
}