using System;

namespace Terraria.ModLoader
{
	public interface IOnUpdateHook
	{
		private static readonly ComponentHook<Action<Component>> Hook = new(typeof(IOnUpdateHook).GetMethod(nameof(OnUpdate)));

		void OnUpdate();

		public static void Invoke(GameObject gameObject) {
			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				function(obj);
			}
		}
	}
}
