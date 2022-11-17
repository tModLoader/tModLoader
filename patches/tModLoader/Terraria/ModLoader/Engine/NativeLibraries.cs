﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader.Engine;

internal class NativeLibraries
{
	internal static void CheckNativeFAudioDependencies() {
		if (!OperatingSystem.IsWindows())
			return;

		try {
			NativeLibrary.Load("vcruntime140.dll", Assembly.GetExecutingAssembly(), DllImportSearchPath.System32);
		}
		catch (DllNotFoundException e) {
			e.HelpLink = "https://www.microsoft.com/en-us/download/details.aspx?id=53587";
			ErrorReporting.FatalExit("Microsoft Visual C++ 2015 Redistributable Update 3 is missing. You will need to download and install it from the Microsoft website.", e);
		}

		try {
			NativeLibrary.Load("mfplat.dll", Assembly.GetExecutingAssembly(), DllImportSearchPath.System32);
		}
		catch (DllNotFoundException e) {
			e.HelpLink = "https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a";
			ErrorReporting.FatalExit("Windows Versions N and KN are missing some media features.\n\nFollow the instructions in the Microsoft website\n\nSearch \"Media Feature Pack list for Windows N editions\" if the page doesn't open automatically.", e);
		}
	}

	internal static void SetNativeLibraryPath(string nativesDir) {
		if (!OperatingSystem.IsWindows())
			return;

		// Setting PATH in the launch scripts is insufficient to guard from bad libs in System32 and similar on windows
		// Add/SetDllDirectory has the highest priority so will pick our shipped natives first
		try {
			SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
			AddDllDirectory(nativesDir);
		}
		catch {
			// Pre-Windows 7, KB2533623 
			SetDllDirectory(nativesDir);
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool SetDefaultDllDirectories(int directoryFlags);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	static extern void AddDllDirectory(string lpPathName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool SetDllDirectory(string lpPathName);

	const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
}