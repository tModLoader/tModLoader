namespace Terraria.ModLoader.Setup.Core.Utilities
{
	public static class PathUtils
	{
		public static string GetCrossPlatformFullPath(string path)
		{
			if (path is "~" or "~/" or "~\\") {
				return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			}

			string result = path;
			if (path.StartsWith("~/") || path.StartsWith("~\\")) {
				result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
			}

			return Path.GetFullPath(result);
		}

		public static string WithUnixSeparators(string path) => path.Replace('\\', '/');

		public static string UnixJoin(params string[] paths) => string.Join("/", paths.Where(x => x.Length > 0));
	}
}