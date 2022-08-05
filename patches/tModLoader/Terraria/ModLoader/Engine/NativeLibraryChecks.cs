using System;
using System.Runtime.InteropServices;
using Terraria.Localization;

namespace Terraria.ModLoader.Engine
{
	internal class NativeLibraryChecks
	{
		internal static void CheckForMSVCRuntimeWindows() {
			if (!OperatingSystem.IsWindows())
				return;

			try { // FAudio on windows needs vcruntime140.dll
				NativeLibrary.Load("vcruntime140.dll");
			}
			catch (DllNotFoundException e) {
				e.HelpLink = "https://www.microsoft.com/en-us/download/details.aspx?id=53587";
				ErrorReporting.FatalExit("Microsoft Visual C++ 2015 Redistributable Update 3 is missing. You will need to download and install it from the Microsoft website.", e);
			}
		}
	}
}