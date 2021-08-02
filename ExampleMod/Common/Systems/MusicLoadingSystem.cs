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

			// When registering music manually, you will
			// have to provide an instance to your mod.
			// You still have to enter the path including
			// your mod's name, though. The extension is also
			// required, but only when adding music.
			MusicLoader.AddMusic(Mod, "ExampleMod/Assets/Sounds/Music/MarbleGallery.ogg");

			// When getting the music slow, you
			// should not add the file extensions!
			Mod.AddMusicBox(MusicLoader.GetMusicSlot("ExampleMod/Assets/Sounds/Music/MarbleGallery"), ModContent.ItemType<ExampleMusicBox>(), ModContent.TileType<ExampleMusicBoxTile>());
		}
	}
}