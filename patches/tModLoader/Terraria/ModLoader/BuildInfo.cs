using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader;

public static class BuildInfo
{
	public enum BuildPurpose
	{
		Dev, // Personal Builds
		Preview, // Monthly preview builds from CI that modders develop against for compatibility
		Stable // The 'stable' builds from CI that players are expected to play on.
	}

	public static readonly string BuildIdentifier = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

	public static readonly Version tMLVersion;
	/// <summary>The Major.Minor version of the stable release at the time this build was created.</summary>
	public static readonly Version stableVersion;
	public static readonly BuildPurpose Purpose;
	public static readonly string BranchName;
	public static readonly string CommitSHA;

	/// <summary>
	/// local time, for display purposes
	/// </summary>
	public static readonly DateTime BuildDate;

	public static bool IsStable => Purpose == BuildPurpose.Stable;
	public static bool IsPreview => Purpose == BuildPurpose.Preview;
	public static bool IsDev => Purpose == BuildPurpose.Dev;


	// SteamApps.GetCurrentBetaName(out string betaName, 100) ? betaName :
	public static readonly string versionedName;
	public static readonly string versionTag;
	public static readonly string versionedNameDevFriendly;

	static BuildInfo()
	{
		var parts = BuildIdentifier.Substring(BuildIdentifier.IndexOf('+') + 1).Split('|');
		int i = 0;

		tMLVersion = new Version(parts[i++]);
		stableVersion = new Version(parts[i++]);
		BranchName = parts[i++];
		Enum.TryParse(parts[i++], true, out Purpose);
		CommitSHA = parts[i++];
		BuildDate = DateTime.FromBinary(long.Parse(parts[i++])).ToLocalTime();

		// Version name for players
		versionedName = $"tModLoader v{tMLVersion}";

		string[] branchNameBlacklist = { "unknown", "stable", "preview", "1.4.3-Legacy" };
		if (!string.IsNullOrEmpty(BranchName) && !branchNameBlacklist.Contains(BranchName))
			versionedName += $" {BranchName}";

		if (Purpose != BuildPurpose.Stable)
			versionedName += $" {Purpose}";

		// Version Tag for ???
		versionTag = versionedName.Substring("tModLoader ".Length).Replace(' ', '-').ToLower();

		// Version name for modders
		versionedNameDevFriendly = versionedName;

		if (CommitSHA != "unknown")
			versionedNameDevFriendly += $" {CommitSHA.Substring(0, 8)}";

		versionedNameDevFriendly += $", built {BuildDate:g}";
	}
}
