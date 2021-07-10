using System;
using System.Reflection;

namespace Terraria.ModLoader
{
	public static class BuildInfo
	{
		public enum BuildPurpose
		{
			Unknown,
			Dev,
			Beta,
			Release
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

		public static bool IsRelease => Purpose == BuildPurpose.Release;


		// SteamApps.GetCurrentBetaName(out string betaName, 100) ? betaName :
		public static readonly string versionedName;
		public static readonly string versionTag;
		public static readonly string versionedNameDevFriendly;

		static BuildInfo() {
			var parts = BuildIdentifier.Substring(BuildIdentifier.IndexOf('+')+1).Split('-');
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


			versionedName = $"tModLoader v{tMLVersion}";

			if (!string.IsNullOrEmpty(BranchName) && BranchName != "master" && BranchName != "unknown")
				versionedName += $" {BranchName}";

			if (Purpose != BuildPurpose.Release)
				versionedName += $" {Purpose}";

			versionTag = versionedName.Substring("tModLoader ".Length).Replace(' ', '-').ToLower();

			versionedNameDevFriendly = versionedName;

			if (CommitSHA != "unknown")
				versionedNameDevFriendly += $" {CommitSHA.Substring(0, 8)}";

			versionedNameDevFriendly += $", built {BuildDate:g}";
		}
	}
}
