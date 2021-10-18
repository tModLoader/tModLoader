using System;

namespace Terraria.ModLoader
{
	public interface ICanBurnInLava
	{
		private static readonly ComponentHook<Func<Component, bool?>> Hook = new(typeof(ICanBurnInLava).GetMethod(nameof(CanBurnInLava)));

		bool? CanBurnInLava();

		public static bool? Invoke(GameObject gameObject) {
			bool? result = null;

			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				bool? currentResult = function(obj);

				switch (currentResult) {
					case null:
						continue;
					case false:
						result = false;
						continue;
					case true:
						return true;
				}
			}

			return result;
		}
	}
}
