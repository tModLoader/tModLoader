using ExampleMod.Content.Items.Placeable;
using ExampleMod.Content.Tiles;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// Example system showing off manual music loading and adding music boxes.
	public class MusicLoadingSystem : ModSystem
	{
		public override void OnModLoad() {
			base.OnModLoad();

			// When registering music manually, you will have to provide an instance to your mod.
			// Since you're providing an instance of your mod, you should not start the path with your mod's name.
			// Accepted music formats are: .mp3, .ogg, and .wav files.
			// Do NOT add the file extension in your code when adding music!
			MusicLoader.AddMusic(Mod, "Assets/Sounds/Music/MarbleGallery");

			// When getting the music slot, you should not add the file extensions!
			MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot("ExampleMod/Assets/Sounds/Music/MarbleGallery"), ModContent.ItemType<ExampleMusicBox>(), ModContent.TileType<ExampleMusicBoxTile>());
		}
	}
}