using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tModLoaderTests")]
namespace Terraria.ModLoader.Core;

internal class LocalMod
{
	public readonly ModLocation location;
	public readonly TmodFile modFile;
	public readonly BuildProperties properties;
	public DateTime lastModified;

	public string Name => modFile.Name;
	public string DisplayName => string.IsNullOrEmpty(properties.displayName) ? Name : properties.displayName;
	public Version Version => properties.version;
	public Version tModLoaderVersion => properties.buildVersion;

	public bool Enabled {
		get => ModLoader.IsEnabled(Name);
		set => ModLoader.SetModEnabled(Name, value);
	}

	public override string ToString() => $"{Name} {Version} for tML {tModLoaderVersion} from {location}";

	public LocalMod(ModLocation location, TmodFile modFile, BuildProperties properties)
	{
		this.location = location;
		this.modFile = modFile;
		this.properties = properties;
	}

	public LocalMod(ModLocation location, TmodFile modFile) : this(location, modFile, BuildProperties.ReadModFile(modFile))
	{
	}
}
