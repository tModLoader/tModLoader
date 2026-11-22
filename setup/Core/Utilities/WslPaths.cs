using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader.Setup.Core.Utilities
{
	/// <summary>
	/// Translates Windows drive paths to their WSL mount points, using WSL's own <c>wslpath</c>.
	/// <para/>
	/// A <c>WorkspaceInfo.targets</c> written by a Windows setup run holds paths like
	/// <c>C:\Program Files (x86)\Steam</c> which we can translate to <c>/mnt/c/Program Files (x86)/Steam</c>
	/// </summary>
	public static class WslPaths
	{
		private static readonly Lazy<bool> Available =
			new(() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Translate("-w", "/") != null);

		private static readonly ConcurrentDictionary<string, string?> Translations = new();

		public static bool TryToUnix(string path, out string unixPath)
		{
			unixPath = path;

			// wslpath translates whatever it is given, including unix paths (/home to /mnt/c/home), so it is
			// only handed rooted drive paths. "C:" and "C:file" are drive-relative and have no meaning here.
			if (path.Length < 3 || !char.IsAsciiLetter(path[0]) || path[1] != ':' || path[2] is not ('\\' or '/'))
				return false;

			return TryTranslate("-u", path, out unixPath!);
		}

		/// <summary>Whether a path lies on a mounted Windows drive, and so holds Windows files.</summary>
		public static bool IsOnWindowsDrive(string path)
		{
			// Paths inside the distro translate to a \\wsl.localhost\ share rather than a drive.
			return TryTranslate("-w", path, out string? windowsPath) && windowsPath.Length > 1 && windowsPath[1] == ':';
		}

		private static bool TryTranslate(string mode, string path, [NotNullWhen(true)] out string? translated)
		{
			translated = default;
			if (!Available.Value)
				return false;

			translated = Translations.GetOrAdd($"{mode} {path}", _ => Translate(mode, path));
			return translated != null;
		}

		private static string? Translate(string mode, string path)
		{
			string? translated = null;

			try {
				// Forward slashes are accepted, and keep a trailing separator from escaping the closing quote.
				// On failure wslpath writes the path it could not translate to stderr, which is discarded.
				int exitCode = RunCmd.Run("", "wslpath", $"{mode} \"{PathUtils.WithUnixSeparators(path)}\"",
					s => translated = s.Trim(), _ => { });

				return exitCode == 0 ? translated : null;
			}
			catch (Win32Exception) { // Not running under WSL
				return null;
			}
		}
	}
}
