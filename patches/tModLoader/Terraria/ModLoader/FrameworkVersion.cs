using Microsoft.Win32;
using ReLogic.OS;
using System;
using System.Reflection;

namespace Terraria.ModLoader;

public enum Framework
{
	NetCore,
	NetFramework,
	Mono,
	Unknown
}

public static class FrameworkVersion
{
#if NETCORE
	public static readonly Framework Framework = Framework.NetCore;
#else
	public static readonly Framework Framework = Framework.NetFramework;
#endif

	public static readonly Version Version = GetVersion();

	private static Version GetVersion()
	{
#if !NETCORE
		const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
		using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
			if (ndpKey != null && ndpKey.GetValue("Release") is int releaseKey)
				return CheckFor45PlusVersion(releaseKey);
#endif
		return Environment.Version;
	}

	// Checking the version using >= will enable forward compatibility.
	private static Version CheckFor45PlusVersion(int releaseKey)
	{
		if (releaseKey >= 528040)
			return new Version("4.8");
		if (releaseKey >= 461808)
			return new Version("4.7.2");
		if (releaseKey >= 461308)
			return new Version("4.7.1");
		if (releaseKey >= 460798)
			return new Version("4.7");
		if (releaseKey >= 394802)
			return new Version("4.6.2");
		if (releaseKey >= 394254)
			return new Version("4.6.1");
		if (releaseKey >= 393295)
			return new Version("4.6");
		if (releaseKey >= 379893)
			return new Version("4.5.2");
		if (releaseKey >= 378675)
			return new Version("4.5.1");
		if (releaseKey >= 378389)
			return new Version("4.5");

		throw new Exception("No 4.5 or later version detected");
	}
}
