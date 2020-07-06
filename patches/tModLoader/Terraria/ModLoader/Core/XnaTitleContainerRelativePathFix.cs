#if XNA
using Ionic.Zip;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.IO;
#endif

namespace Terraria.ModLoader.Core
{
	internal static class XnaTitleContainerRelativePathFix
	{
		/// <summary>
		/// TitleContainer.OpenStream throws an error if it's fed a path that starts with '..\', treating paths like that as absolute, for some reason.
		/// </summary>
		public static void Init() {
#if XNA
			//Can't believe that this is enough, lmao.
			HookEndpointManager.Add(typeof(TitleContainer).FindMethod("IsCleanPathAbsolute"), new Func<string, bool>(path => false));
#endif
		}

	}
}
