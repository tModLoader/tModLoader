using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace Terraria.ModLoader.Engine;

internal class ControlledFolderAccessSupport
{
	// Some background and consulted resources
	//https://modding.wiki/en/vortex/users/controlled-folder-access
	//https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.registry.getvalue?view=net-6.0
	//https://stackoverflow.com/questions/56703623/how-to-check-for-directory-write-permission-in-net-when-controlled-folder-acce

	internal static bool ControlledFolderAccessDetected;
	internal static bool ControlledFolderAccessDetectionPrevented;
	internal static void CheckFileSystemAccess()
	{
		try {
			if (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major >= 10)
				CheckRegistryValues();
		}
		catch {
			ControlledFolderAccessDetectionPrevented = true;
		}
	}

	[SupportedOSPlatform("windows")]
	private static void CheckRegistryValues()
	{
		if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access", "EnableControlledFolderAccess", -1) is int EnableControlledFolderAccessValue) {
			ControlledFolderAccessDetected = EnableControlledFolderAccessValue == 1;
		}

		// This is as far as we can go in detecting the underlying issues. We can't check AllowedApplications and ProtectedFolders
		// Attempting to view \Controlled Folder Access\AllowedApplications prevented by OS.
		// \Controlled Folder Access\ProtectedFolders doesn't matter, automatic default save locations like the documents folder won't appear there
	}
}