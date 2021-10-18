namespace Terraria.ModLoader
{
	[ComponentHook]
	public partial interface IOnUpdateHook
	{
		void OnUpdate();

		public static void Invoke(GameObject gameObject) {
			foreach (var (obj, function) in HookOnUpdate.Enumerate(gameObject)) {
				function(obj);
			}
		}
	}
}
