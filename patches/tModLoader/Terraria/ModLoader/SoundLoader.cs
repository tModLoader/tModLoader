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
			const string SoundsFolder = "Sounds";

			// Do some preloading here to avoid stuttering when playing a sound ingame
			foreach (string path in mod.RootContentSource.EnumerateAssets().Where(s => s.StartsWith($"{SoundsFolder}/") || s.Contains($"/{SoundsFolder}/"))) {
				string soundPath = Path.ChangeExtension(path, null);

				mod.Assets.Request<SoundEffect>(soundPath, AssetRequestMode.AsyncLoad);
			}
		}

		internal static void ResizeAndFillArrays() {
			
		}

		internal static void Unload() {
			
		}
	}
}