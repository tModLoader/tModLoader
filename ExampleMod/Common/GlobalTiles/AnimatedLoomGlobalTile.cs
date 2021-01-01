using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	//This class showcases how to change the vanilla loom texture and also add animation for our own texture
	public class AnimatedLoomGlobalTile : GlobalTile
	{
		public const int FrameCount = 4;
		public const int FrameSpeed = 16;

		//We have to cache the textures we work with so we can reference them easier
		private static Asset<Texture2D> originalLoom;

		private static Asset<Texture2D> animatedLoom;

		private static bool replacedTexture = false;

		public override void SetupContent() {
			//This hook is ran once on mod load, after all content has been loaded into the game, to be sure that everything exists
			if (!Main.dedServ) {
				//Do not try to do texture stuff on the server!
				int type = TileID.Loom;

				Main.instance.LoadTiles(type); //Loads the tile texture
				originalLoom = TextureAssets.Tile[type]; //Cache the tile texture so we can restore it later on unload

				animatedLoom = ModContent.GetTexture("ExampleMod/Common/GlobalTiles/AnimatedLoom"); //Fetch our own texture
				TextureAssets.Tile[type] = animatedLoom; //Replace the vanilla texture with ours
				replacedTexture = true; //Flag it so we can be sure we replaced it
			}
		}

		public override void AnimateTile() {
			int type = TileID.Loom;

			//Simply cycle through the four frames of the AnimatedLoom spritesheet:
			//Spend FrameSpeed amount of ticks on each frame
			//Roll over to the first frame after reaching the FrameCount
			//(both values are defined at the top of the class)
			if (++Main.tileFrameCounter[type] >= FrameSpeed) {
				Main.tileFrameCounter[type] = 0;

				if (++Main.tileFrame[type] >= FrameCount) {
					Main.tileFrame[type] = 0;
				}
			}
		}

		public override void Unload() {
			if (!Main.dedServ) {
				//Do not try to do texture stuff on the server!
				int type = TileID.Loom;

				if (replacedTexture) {
					//We previously set this to true if our replacement was successful

					Main.tileFrame[type] = 0; //Reset the frame of the loom tile
					TextureAssets.Tile[type] = originalLoom; //Restore the original texture
					replacedTexture = false; //Unset the flag
				}

				//We hold a reference to the now reverted texture, so simply null it for us; calling Dispose() would also dispose it in TextureAssets.Tile!
				originalLoom = null;

				if (animatedLoom != null) {
					//Dispose and null our own texture
					animatedLoom.Dispose();

					animatedLoom = null;
				}
			}
		}
	}
}
