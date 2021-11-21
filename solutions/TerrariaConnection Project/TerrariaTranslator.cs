using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace TerrariaTranslator
{
    class TerrariaTranslator
    {

		public static readonly object resolverLock = new object();
		private static readonly Dictionary<string, IntPtr> assemblies = new Dictionary<string, IntPtr>();

		static void Main(string[] Args)
        {
            AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolveNativeLibrary;

            Thread.Sleep(2000);
            Console.WriteLine(Directory.GetCurrentDirectory());

            bool unloading = !SteamAPI.Init();

            while (!unloading) {
                string nextCMD = Console.ReadLine();

                if (nextCMD.Contains("unload"))
                    unloading = true;

                if (nextCMD.Contains("grant:")) {
                    string achievement = nextCMD.Split(':')[1];

                    SteamUserStats.GetAchievement(achievement, out bool pbAchieved);
                    if (!pbAchieved)
                        SteamUserStats.SetAchievement(achievement);
                }
            }

            Environment.Exit(0);
        }

		private static IntPtr ResolveNativeLibrary(Assembly assembly, string name)
		{
			lock (resolverLock)
			{
				try
				{
					if (assemblies.TryGetValue(name, out var handle))
					{
						return handle;
					}

					var dir = Path.Combine("../", "Native", getNativeDir(name));
					var files = Directory.GetFiles(dir, $"*{name}*", SearchOption.AllDirectories);
					var match = files.FirstOrDefault();

					if (match != null && NativeLibrary.TryLoad(match, out handle))
					{
						return assemblies[name] = handle;
					}
					else
					{
						// Toss an error when failed to load needed library file, instead of just waiting for later to toss - Solxan
						assemblies[name] = IntPtr.Zero;
						throw new FileLoadException("Failed to load Native Library at " + match);
					}
				}
				catch (DirectoryNotFoundException e)
				{
					throw new DirectoryNotFoundException("A needed library file was missing from the tModLoader directory. " + e.Message, e);
				}
			}
		}

		private static string getNativeDir(string name)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "Windows";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return "Linux";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return "OSX";
			throw new InvalidOperationException("Unknown OS.");
		}
	}
}
