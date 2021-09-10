using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary> This class is used to keep track of and support the existence of custom sounds that have been added to the game. </summary>
	//TODO: Load asynchronously and on demand.
	public static class SoundLoader
	{
		internal static void AutoloadSounds(Mod mod) {
			// do some preloading here to avoid stuttering when playing a sound ingame
			foreach (string path in mod.RootContentSource.EnumerateAssets().Where(s => s.Contains("Sounds" + Path.DirectorySeparatorChar))) {
				mod.Assets.Request<SoundEffect>(path, AssetRequestMode.AsyncLoad);
			}
		}

		internal static void ResizeAndFillArrays() {
			
		}

		internal static void Unload() {
			
		}
	}
}