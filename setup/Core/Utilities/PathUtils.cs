namespace Terraria.ModLoader.Setup.Core.Utilities
{
	public static class PathUtils
	{
		public static string GetCrossPlatformFullPath(string path)
		{
			if (path == "~") {
				return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			}

			string result = path;
			if (path.StartsWith('~')) {
				result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
			}

			return Path.GetFullPath(result);
		}

		public static string SetCrossPlatformDirectorySeparators(string path) => path.Replace('\\', '/');
	}
}