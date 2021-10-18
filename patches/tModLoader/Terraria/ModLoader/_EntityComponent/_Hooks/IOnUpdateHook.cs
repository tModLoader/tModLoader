namespace Terraria.ModLoader
{
	[ComponentHook]
	public partial interface IOnUpdateHook
	{
		void OnUpdate();

		public static void Invoke(GameObject gameObject) {
			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				function(obj);
			}
		}
	}
}
