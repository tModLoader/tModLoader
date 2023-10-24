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
		TimeSpan span = DateTime.Now - lastModified;

		if (span.Days > 365) {
			int years = (span.Days / 365);
			if (span.Days % 365 != 0)
				years += 1;
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), years, years == 1 ? "year" : "years");
		}
		if (span.Days > 30) {
			int months = (span.Days / 30);
			if (span.Days % 31 != 0)
				months += 1;
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), months, months == 1 ? "month" : "months");
		}
		if (span.Days > 0)
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), span.Days, span.Days == 1 ? "day" : "days");
		if (span.Hours > 0)
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), span.Hours, span.Hours == 1 ? "hour" : "hours");
		if (span.Minutes > 0)
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
		if (span.Seconds > 5)
			return string.Format(Language.GetTextValue("tModLoader.MSTimeSinceBuild"), span.Seconds, span.Seconds == 1 ? "second" : "seconds");
		if (span.Seconds <= 5)
			return Language.GetTextValue("tModLoader.MSLastBuildJustNow");
		return string.Empty;
	}
}
