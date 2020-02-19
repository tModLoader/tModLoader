using Microsoft.Win32;
using ReLogic.OS;
using System;
using System.Diagnostics;
using System.IO;

namespace Terraria.ModLoader.Core
{
	internal class FileAssociationSupport
	{
		internal static void HandleFileAssociation(string file) {
			Console.WriteLine($"Attempting to install {file}");
			if (File.Exists(file)) {
				string modName = Path.GetFileNameWithoutExtension(file);
				if (ModLoader.ModPath != Path.GetDirectoryName(file)) {
					File.Copy(file, Path.Combine(ModLoader.ModPath, Path.GetFileName(file)), true);
					File.Delete(file);
					Console.WriteLine($"{modName} installed successfully");
				}
				ModLoader.EnableMod(modName);
				Console.WriteLine($"{modName} enabled");
			}
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
			Environment.Exit(0);
		}

		internal static void UpdateFileAssociation() {
			if (Platform.IsWindows && System.Environment.OSVersion.Version.Major >= 6) // Approached used apparently only applicable to Vista and later
				try {
					// For some reason this has been reported as failing occasionally.
					EnsureAssociationsSet();
				}
				catch (Exception) {
				}
		}

		// Solution below adapted from https://stackoverflow.com/a/44816953
		private class FileAssociation
		{
			public string Extension { get; set; }
			public string ProgId { get; set; }
			public string FileTypeDescription { get; set; }
			public string ExecutableFilePath { get; set; }
		}

		// needed so that Explorer windows get refreshed after the registry is updated
		[System.Runtime.InteropServices.DllImport("Shell32.dll")]
		private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

		private const int SHCNE_ASSOCCHANGED = 0x8000000;
		private const int SHCNF_FLUSH = 0x1000;

		private static void EnsureAssociationsSet() {
			var filePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "tModLoaderServer.exe");
			EnsureAssociationsSet(
				new FileAssociation {
					Extension = ".tmod",
					ProgId = "tModLoader_Mod_File",
					FileTypeDescription = "tModLoader Mod",
					ExecutableFilePath = filePath
				});
		}

		private static void EnsureAssociationsSet(params FileAssociation[] associations) {
			bool madeChanges = false;
			foreach (var association in associations) {
				madeChanges |= SetAssociation(
					association.Extension,
					association.ProgId,
					association.FileTypeDescription,
					association.ExecutableFilePath);
			}
			if (madeChanges) {
				SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
			}
		}

		private static bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath) {
			bool madeChanges = false;
			madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
			madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
			madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", $"\"{applicationFilePath}\" -install \"%1\"");
			return madeChanges;
		}

		private static bool SetKeyDefaultValue(string keyPath, string value) {
			using (var key = Registry.CurrentUser.CreateSubKey(keyPath)) {
				if (key.GetValue(null) as string != value) {
					key.SetValue(null, value);
					return true;
				}
			}
			return false;
		}
	}
}
