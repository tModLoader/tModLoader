using Ionic.Zip;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.IO;

namespace Terraria.ModLoader.Core
{
	internal static class ZipExtractFix
	{
		/// <summary>
		/// When Ionic.Zip extracts an entry it uses \\ for all separators when it should use Path.DirectorySeparatorChar for platform compatibility
		/// </summary>
		public static void Init() {
			if (Path.DirectorySeparatorChar == '\\')
				return;

			HookEndpointManager.Modify(typeof(ZipEntry).FindMethod("ValidateOutput"), new Action<ILContext>(il => {
				var c = new ILCursor(il);
				c.Next = null; // move to end

				// outFileName = outFileName.Replace("/", "\\")
				c.GotoPrev(MoveType.Before, 
					i => i.MatchLdstr("\\"),
					i => i.MatchCallvirt<string>("Replace"));

				// replace the constant "\\" with Path.DirectorySeparatorChar
				c.Next.Operand = Path.DirectorySeparatorChar.ToString();
			}));
		}
	}
}
