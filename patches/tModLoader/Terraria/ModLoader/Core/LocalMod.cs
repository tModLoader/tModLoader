using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Terraria.Localization;

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
	public string DisplayName => GetLocalizedDisplayName();
	public string DisplayNameClean => Utils.CleanChatTags(DisplayName); // Suitable for console output, chat tags stripped away.
	public Version Version => properties.version;
	public Version tModLoaderVersion => properties.buildVersion;

	public bool Enabled {
		get => ModLoader.IsEnabled(Name);
		set => ModLoader.SetModEnabled(Name, value);
	}

	public override string ToString() => Name;

	private string GetLocalizedDisplayName()
	{
		string cultureName = Language.ActiveCulture?.Name;
		if (!string.IsNullOrEmpty(cultureName) && properties.localizedDisplayNames.TryGetValue(cultureName, out string localizedDisplayName) && !string.IsNullOrWhiteSpace(localizedDisplayName)) {
			return localizedDisplayName;
		}

		return string.IsNullOrEmpty(properties.displayName) ? Name : properties.displayName;
	}

	public string GetDescription()
	{
		if (!TryReadLocalizedDescription(out string description)) {
			description = properties.description;
		}

		ModCompile.UpdateSubstitutedDescriptionValues(ref description, properties.version.ToString(), properties.homepage);
		return description;
	}

	private bool TryReadLocalizedDescription(out string description)
	{
		description = null;
		string cultureName = Language.ActiveCulture?.Name;
		if (!string.IsNullOrEmpty(cultureName) && TryReadDescriptionFile($"description_{cultureName}.txt", out description)) {
			return true;
		}

		return false;
	}

	private bool TryReadDescriptionFile(string fileName, out string description)
	{
		description = null;
		string actualFileName = modFile.GetFileNames().FirstOrDefault(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase));
		if (actualFileName == null) {
			return false;
		}

		using (modFile.Open()) {
			description = Encoding.UTF8.GetString(modFile.GetBytes(actualFileName));
		}

		return !string.IsNullOrWhiteSpace(description);
	}

	public string DetailedInfo => $"{Name} {Version} for tML {tModLoaderVersion} from {location}" + (Path.GetFileNameWithoutExtension(modFile.path) != Name ? $" ({Path.GetFileName(modFile.path)})": "");

	public LocalMod(ModLocation location, TmodFile modFile, BuildProperties properties)
	{
		this.location = location;
		this.modFile = modFile;
		this.properties = properties;
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
