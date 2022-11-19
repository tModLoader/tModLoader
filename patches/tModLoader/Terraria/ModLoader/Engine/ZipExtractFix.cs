using Ionic.Zip;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.IO;

namespace Terraria.ModLoader.Engine;

internal static class ZipExtractFix
{
	private static ILHook hook;

	/// <summary>
	/// When Ionic.Zip extracts an entry it uses \\ for all separators when it should use Path.DirectorySeparatorChar for platform compatibility
	/// </summary>
	public static void Init()
	{
		if (Path.DirectorySeparatorChar == '\\')
			return;

		hook = new ILHook(typeof(ZipEntry).FindMethod("ValidateOutput"), il => {
			var c = new ILCursor(il);
			c.Next = null; // move to end

			// outFileName = outFileName.Replace("/", "\\")
			c.GotoPrev(MoveType.Before,
				i => i.MatchLdstr("\\"),
				i => i.MatchCallvirt<string>("Replace"));

			// replace the constant "\\" with Path.DirectorySeparatorChar
			c.Next.Operand = Path.DirectorySeparatorChar.ToString();
		});
	}
}
