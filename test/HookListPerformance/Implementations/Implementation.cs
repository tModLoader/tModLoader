using System;

namespace HookListPerformance
{
	abstract class Implementation
	{
		public abstract float HookDoEffect(float input, Item[] items, int[] hookInds);
	}
}