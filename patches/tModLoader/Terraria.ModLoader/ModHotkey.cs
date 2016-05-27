namespace Terraria.ModLoader
{
	class ModHotkey
	{
		// RegisterHotKey("Random Buff", "P");
		internal Mod mod;
		internal string name; // name from modder: "Random Buff"
		internal string displayName; // display name: "Example Mod: Random Buff" -- unique?
		//internal string currentKey; // from config or default
		internal string defaultKey; // from mod.Load
		//string configurationString; // Prevent 2 mods from having same hotkey

		public ModHotkey(string name, Mod mod, string defaultKey)
		{
			this.name = name;
			this.mod = mod;
			//this.currentKey = currentKey;
			this.defaultKey = defaultKey;
			this.displayName = mod.Name + ": " + name;
			//configurationString = mod.Name + "_" + "HotKey" + "_" + name.Replace(' ', '_');
		}
	}
}
