namespace Terraria.ModLoader
{
	/// <summary>
	/// This is a struct that stores the properties of a mod. Without setting it in your Mod constructor, all properties default to true.
	/// </summary>
	public struct ModProperties
	{
		/// <summary>
		/// Whether or not this mod will autoload content by default. Autoloading content means you do not need to manually add content through methods.
		/// </summary>
		public bool Autoload;
		/// <summary>
		/// Whether or not this mod will automatically add images in the Gores folder as gores to the game, along with any ModGore classes that share names with the images. This means you do not need to manually call Mod.AddGore.
		/// </summary>
		public bool AutoloadGores;
		/// <summary>
		/// Whether or not this mod will automatically add sounds in the Sounds folder to the game. Place sounds in Sounds/Item to autoload them as item sounds, Sounds/NPCHit to add them as npcHit sounds, Sounds/NPCKilled to add them as npcKilled sounds, and Sounds/Music to add them as music tracks. Sounds placed anywhere else in the Sounds folder will be added as custom sounds. Any ModSound classes that share the same name as the sound files will be bound to them. Setting this field to true means that you do not need to manually call AddSound.
		/// </summary>
		public bool AutoloadSounds;
		/// <summary>
		/// Whether or not this mod will automatically add images in the Backgrounds folder as background textures to the game. This means you do not need to manually call Mod.AddBackgroundTexture.
		/// </summary>
		public bool AutoloadBackgrounds;

		/// <summary>
		/// Automatically return a ModProperties object which has all AutoLoad values set to true.
		/// </summary>
		public static ModProperties AutoLoadAll { get; } =
			new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true,
				AutoloadBackgrounds = true
			};
	}
}
