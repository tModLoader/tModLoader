using System;
using System.Runtime.CompilerServices;
using Terraria.Localization;

[assembly: InternalsVisibleTo("tModLoaderTests")]
namespace Terraria.ModLoader.Core;

internal class LocalMod
{
	public readonly TmodFile modFile;
	public readonly BuildProperties properties;
	public DateTime lastModified;

	public string Name => modFile.Name;
	public string DisplayName => string.IsNullOrEmpty(properties.displayName) ? Name : properties.displayName;
	public Version tModLoaderVersion => properties.buildVersion;

	public bool Enabled {
		get => ModLoader.IsEnabled(Name);
		set => ModLoader.SetModEnabled(Name, value);
	}

	public override string ToString() => Name;

	public LocalMod(TmodFile modFile, BuildProperties properties)
	{
		this.modFile = modFile;
		this.properties = properties;
	}

	public LocalMod(TmodFile modFile) : this(modFile, BuildProperties.ReadModFile(modFile))
	{
	}

	public string GetTimeSinceLastBuilt()
	{
		TimeSpan diff = DateTime.Now - lastModified;

		if (diff.Days > 365) {
			int years = (diff.Days / 365);
			if (diff.Days % 365 != 0)
				years += 1;
			return Language.GetTextValue("tModLoader.MSLastBuildXYears", years);
		}
		if (diff.Days > 30) {
			int months = (diff.Days / 30);
			if (diff.Days % 31 != 0)
				months += 1;
			return Language.GetTextValue("tModLoader.MSLastBuildXMonths", months);
		}
		if (diff.Days > 0)
			return Language.GetTextValue("tModLoader.MSLastBuildXDays", diff.Days);
		if (diff.Hours > 0)
			return Language.GetTextValue("tModLoader.MSLastBuildXHours", diff.Hours);
		if (diff.Minutes > 0)
			return Language.GetTextValue("tModLoader.MSLastBuildXMinutes", diff.Minutes);
		if (diff.Seconds > 5)
			return Language.GetTextValue("tModLoader.MSLastBuildXSeconds", diff.Seconds);
		if (diff.Seconds <= 5)
			return Language.GetTextValue("tModLoader.MSLastBuildJustNow");
		return string.Empty;
	}
}
