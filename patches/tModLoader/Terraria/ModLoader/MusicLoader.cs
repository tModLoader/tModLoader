using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public sealed class MusicLoader : ILoader
	{
		internal static readonly Dictionary<int, int> musicToItem = new();
		internal static readonly Dictionary<int, int> itemToMusic = new();
		internal static readonly Dictionary<int, Dictionary<int, int>> tileToMusic = new();

		private static readonly Dictionary<string, int> musicByPath = new();

		private static int nextMusicId = MusicID.Count;

		private void Autoload(Mod mod) {
			/*foreach (string music in mod.musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music.Substring("Sounds/".Length);

				if (substring.StartsWith("Music/")) {
					AddSound(SoundType.Music, Name + '/' + music);
				}
			}*/
		}

		void ILoader.ResizeArrays() {

		}

		void ILoader.Unload() {
			musicToItem.Clear();
			itemToMusic.Clear();
			tileToMusic.Clear();
		}
	}
}
