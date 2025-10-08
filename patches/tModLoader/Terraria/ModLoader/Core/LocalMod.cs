using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tModLoaderTests")]
namespace Terraria.ModLoader.Core;

[DebuggerDisplay("{DetailedInfo}")]
internal class LocalMod
{
	public readonly ModLocation location;
	public readonly TmodFile modFile;
	public readonly BuildProperties properties;
	public DateTime lastModified;

	public string Name => modFile.Name;
	public string DisplayName => string.IsNullOrEmpty(properties.displayName) ? Name : properties.displayName;
	public readonly string DisplayNameClean; // Suitable for console output, chat tags stripped away.
	public Version Version => properties.version;
	public Version tModLoaderVersion => properties.buildVersion;

	public bool Enabled {
		get => ModLoader.IsEnabled(Name);
		set => ModLoader.SetModEnabled(Name, value);
	}

	public override string ToString() => Name;

	public string DetailedInfo => $"{Name} {Version} for tML {tModLoaderVersion} from {location}" + (Path.GetFileNameWithoutExtension(modFile.path) != Name ? $" ({Path.GetFileName(modFile.path)})": "");

	public LocalMod(ModLocation location, TmodFile modFile, BuildProperties properties)
	{
		this.location = location;
		this.modFile = modFile;
		this.properties = properties;
		DisplayNameClean = Utils.CleanChatTags(DisplayName);
	}

	public LocalMod(ModLocation location, TmodFile modFile) : this(location, modFile, BuildProperties.ReadModFile(modFile))
	{
	}

	internal static LocalMod FromWorkshopModFile(string path)
	{
		var sModFile = new TmodFile(path);
		using (sModFile.Open())
			return new LocalMod(ModLocation.Workshop, sModFile);
	}
}
