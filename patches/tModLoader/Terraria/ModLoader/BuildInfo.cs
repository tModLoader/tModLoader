using System;
using System.Reflection;

namespace Terraria.ModLoader
{
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

		static BuildInfo() {
			var parts = BuildIdentifier.Substring(BuildIdentifier.IndexOf('+')+1).Split('|');
			tMLVersion = new Version(parts[0]);
			if (parts.Length>=2) {
				BranchName = parts[1];
			}
			else {
				BranchName = "unknown";
			}
			if (parts.Length>=3) {
				Enum.TryParse(parts[2], true, out Purpose);
			}
			if (parts.Length>=4) {
				CommitSHA = parts[3];
			}
			else {
				CommitSHA = "unknown";
			}
			if (parts.Length>=5) {
				BuildDate = DateTime.FromBinary(long.Parse(parts[4])).ToLocalTime();
			}

			// Version name for players
			versionedName = $"tModLoader v{tMLVersion}";

			if (!string.IsNullOrEmpty(BranchName) && BranchName != "unknown"
				&& BranchName != "1.4-stable" && BranchName != "1.4-preview" && BranchName != "1.4")
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
}
