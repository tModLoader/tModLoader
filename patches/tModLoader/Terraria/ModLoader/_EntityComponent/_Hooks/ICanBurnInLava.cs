namespace Terraria.ModLoader
{
	/// <inheritdoc cref="CanBurnInLava"/>
	[ComponentHook]
	public partial interface ICanBurnInLavaHook
	{
		/// <summary>
		/// Returns whether or not this item will burn in lava regardless of any conditions. Returns null by default (follow vanilla behaviour).
		/// </summary>
		bool? CanBurnInLava();

		public static bool? Invoke(GameObject gameObject) {
			bool? result = null;

			foreach (var (obj, function) in HookCanBurnInLava.Enumerate(gameObject)) {
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
