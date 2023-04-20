using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Text;

namespace Terraria.ModLoader.Engine;

internal class FolderShortcutSupport
{
	internal static void UpdateFolderShortcuts()
	{
		if (OperatingSystem.IsWindows()) {
			try {
				// This is the first file system writing we attempt in the usual documents folder.
				CreateLogsFolderShortcut();
			}
			catch (Exception ex) when (ex is COMException || ex is FileNotFoundException) { 
				if (ControlledFolderAccessSupport.ControlledFolderAccessDetected) {
					Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#controlled-folder-access");
					throw new Exception($"\n\nControlled Folder Access feature detected.\n\nIf game fails to launch make sure to add \"{Environment.ProcessPath}\" to the \"Allow an app through Controlled folder access\" menu found in the \"Ransomware protection\" menu.\n\nMore instructions can be found in the website that just opened.\n\n");  // Before language is loaded, no need to localize
				}
			}
			catch (Exception e) {
				Logging.tML.Warn($"Unable to create logs folder shortcuts - an exception of type {e.GetType().Name} was thrown:\r\n'{e.Message}'.");
			}
		}
	}

	// Implementation below adapted from https://stackoverflow.com/a/14632782
	[SupportedOSPlatform("windows")]
	private static void CreateLogsFolderShortcut()
	{
		IShellLink link = (IShellLink)new ShellLink();

		// setup shortcut information
		link.SetDescription("tModLoader Logs Folder");
		string path = Path.GetFullPath(Logging.LogDir);
		link.SetPath(path);

		// save it
		IPersistFile file = (IPersistFile)link;
		string fullSavePath = Path.GetFullPath(Program.SavePath);
		
		Directory.CreateDirectory(fullSavePath);

		file.Save(Path.Combine(fullSavePath, "Logs.lnk"), false);
	}

	[ComImport]
	[Guid("00021401-0000-0000-C000-000000000046")]
	internal class ShellLink
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	internal interface IShellLink
	{
		void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
		void GetIDList(out IntPtr ppidl);
		void SetIDList(IntPtr pidl);
		void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
		void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
		void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
		void GetHotkey(out short pwHotkey);
		void SetHotkey(short wHotkey);
		void GetShowCmd(out int piShowCmd);
		void SetShowCmd(int iShowCmd);
		void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
		void Resolve(IntPtr hwnd, int fFlags);
		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}
}