using System;
using System.IO;
using System.Runtime.InteropServices;

internal static class FileOriginChecker
{
	// --- Linux / macOS Native Interop ---
	[DllImport("libc", EntryPoint = "getxattr", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern long getxattr(string path, string name, byte[] value, ulong size);

	internal static bool IsDownloadedFile(string filePath, out string originUrl)
	{
		originUrl = string.Empty;

		if (!File.Exists(filePath))
			throw new FileNotFoundException("Target file not found.", filePath);

		if (OperatingSystem.IsWindows()) {
			return CheckWindowsZoneIdentifier(filePath, out originUrl);
		}
		/* Untested, need to verify typical browser behavior on Linux and macOS and adjust if needed
		else if (OperatingSystem.IsLinux()) {
			// Some browsers on Linux set user.xdg.origin.url
			return CheckUnixExtendedAttribute(filePath, "user.xdg.origin.url", out originUrl);
		}
		else if (OperatingSystem.IsMacOS()) {
			// macOS uses com.apple.metadata:kMDItemWhereFroms
			// Or just check com.apple.quarantine maybe?
			return CheckUnixExtendedAttribute(filePath, "com.apple.metadata:kMDItemWhereFroms", out originUrl);
		}
		*/

		return false;
	}

	internal static void ClearDownloadFlags(string filePath)
	{
		if (!File.Exists(filePath))
			throw new FileNotFoundException("Target file not found.", filePath);

		if (OperatingSystem.IsWindows()) {
			ClearWindowsZoneIdentifier(filePath);
		}
		else {
			throw new NotImplementedException();
		}
	}


	private static bool CheckWindowsZoneIdentifier(string filePath, out string originUrl)
	{
		originUrl = string.Empty;

		// Windows NTFS Alternate Data Streams (ADS) to store some file metadata.
		string adsPath = $"{filePath}:Zone.Identifier";

		if (File.Exists(adsPath)) {
			try {
				string content = File.ReadAllText(adsPath);

				// Parse standard ZoneId (ZoneId=3 means Internet, ZoneId=4 means Untrusted internet sites)
				if (content.Contains("ZoneId=3") || content.Contains("ZoneId=4")) {
					// Attempt to extract HostUrl if modern browsers saved it
					foreach (var line in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)) {
						// ReferrerUrl= is another option
						if (line.StartsWith("HostUrl=", StringComparison.OrdinalIgnoreCase)) {
							originUrl = line.Substring(8).Trim();
							// Remove fragment (#) and query (?) components from url
							var uri = new Uri(originUrl);
							originUrl = $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
							break;
						}
					}
					return true;
				}
			}
			catch (Exception) {
			}
		}
		return false;
	}

	private static void ClearWindowsZoneIdentifier(string filePath)
	{
		string adsPath = $"{filePath}:Zone.Identifier";
		if (File.Exists(adsPath)) {
			File.Delete(adsPath);

			// Causes the mod to be seen as changed, refreshing the LocalMod
			File.SetLastWriteTime(filePath, DateTime.Now);
		}
	}

	/* Untested code for other OS from search results
	private static bool CheckUnixExtendedAttribute(string filePath, string attributeName, out string originUrl)
	{
		originUrl = string.Empty;
		try {
			// First pass: Call with size 0 to get the exact buffer size required
			long size = getxattr(filePath, attributeName, null, 0);
			if (size <= 0)
				return false;

			byte[] buffer = new byte[size];
			long readBytes = getxattr(filePath, attributeName, buffer, (ulong)size);

			if (readBytes > 0) {
				// Linux: Raw URL string
				// macOS: Typically a binary Apple Property List (.plist). 
				// Reading it as UTF-8 often yields plain-text URLs embedded inside.
				originUrl = Encoding.UTF8.GetString(buffer, 0, (int)readBytes).Trim('\0');
				return true;
			}
		}
		catch (Exception) {
		}
		return false;
	}
	*/
}
