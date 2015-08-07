namespace Terraria.ModLoader.Setup
{
	public class ProgramSetting<T>
	{
		public readonly string key;

		public ProgramSetting(string key) {
			this.key = key;
		}

		public void Set(T value) {
			Properties.Settings.Default[key] = value;
			Properties.Settings.Default.Save();
		}

		public T Get() {
			return (T)Properties.Settings.Default[key];
		}
	}
}