namespace Terraria.ModLoader
{
	public struct ModProperties
	{
		public bool Autoload;
		public bool AutoloadGores;
		public bool AutoloadSounds;
		public bool AutoloadBackgrounds;

		internal static ModProperties AutoLoadAll = new ModProperties()
		{
			Autoload = true,
			AutoloadGores = true,
			AutoloadSounds = true,
			AutoloadBackgrounds = true
		};
	}
}
