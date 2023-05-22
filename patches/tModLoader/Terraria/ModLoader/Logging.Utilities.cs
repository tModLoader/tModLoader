using System;
using System.Diagnostics;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static partial class Logging
{
	private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

	internal static readonly FieldInfo f_fileName = typeof(StackFrame).GetField("_fileName", InstanceNonPublic) ?? typeof(StackFrame).GetField("strFileName", InstanceNonPublic);

	private static readonly Assembly terrariaAssembly = Assembly.GetExecutingAssembly();

	public static void PrettifyStackTraceSources(StackFrame[] frames)
	{
		if (frames == null)
			return;

		foreach (var frame in frames) {
			string filename = frame.GetFileName();
			var assembly = frame.GetMethod()?.DeclaringType?.Assembly;

			if (filename == null || assembly == null)
				continue;

			string trim;

			if (AssemblyManager.GetAssemblyOwner(assembly, out string modName))
				trim = modName;
			else if (assembly == terrariaAssembly)
				trim = "tModLoader";
			else
				continue;

			int index = filename.LastIndexOf(trim, StringComparison.InvariantCultureIgnoreCase);

			if (index > 0) {
				filename = filename.Substring(index);
				f_fileName.SetValue(frame, filename);
			}
		}
	}

	private static void TryFreeingMemory()
	{
		// In case of OOM, unload the Main.tile array and do immediate garbage collection.
		// If we don't do this, there will be a big chance that this method will fail to even quit the game, due to another OOM exception being thrown.

		Main.tile = new Tilemap(0, 0);
		GC.Collect();
	}
}
