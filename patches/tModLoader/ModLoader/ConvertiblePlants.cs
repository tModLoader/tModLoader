using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a type of modded tree. The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted. This class encapsulates several functions that distinguish each type of tree from each other. Use ModTile.SetModTree or GlobalTile.AddModTree to make a tile able to grow this kind of tree.
	/// </summary>
	public abstract class ModTree
	{
		/// <summary>
		/// Return the type of dust created when this tree is destroyed. Returns 7 by default.
		/// </summary>
		/// <returns></returns>
		public virtual int CreateDust() {
			return 7;
		}

		/// <summary>
		/// Return the type of gore created to represent leaves when this tree grows on-screen. Returns -1 by default.
		/// </summary>
		/// <returns></returns>
		public virtual int GrowthFXGore() {
			return -1;
		}

		/// <summary>
		/// Whether or not this tree can drop acorns. Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool CanDropAcorn() {
			return true;
		}

		/// <summary>
		/// The ID of the item that is dropped in bulk when this tree is destroyed.
		/// </summary>
		/// <returns></returns>
		public abstract int DropWood();

		/// <summary>
		/// Return the texture that represents the tile sheet used for drawing this tree.
		/// </summary>
		/// <returns></returns>
		public abstract Texture2D GetTexture();

		/// <summary>
		/// Return the texture containing the possible tree tops that can be drawn above this tree.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="frame"></param>
		/// <param name="frameWidth"></param>
		/// <param name="frameHeight"></param>
		/// <param name="xOffsetLeft"></param>
		/// <param name="yOffset"></param>
		/// <returns></returns>
		public abstract Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight,
			ref int xOffsetLeft, ref int yOffset);

		/// <summary>
		/// Return the texture containing the possible tree branches that can be drawn next to this tree. The trunkOffset parameter can be added to i to get the x-coordinate of the tree's trunk.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="trunkOffset"></param>
		/// <param name="frame"></param>
		/// <returns></returns>
		public abstract Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame);
	}

	/// <summary>
	/// This class represents a type of modded palm tree. The palm tree will share a tile ID with the vanilla palm trees (323), so that the trees can freely convert between each other if the sand below is converted. This class encapsulates several functions that distinguish each type of palm tree from each other. Use ModTile.SetModPalmTree or GlobalTile.AddModPalmTree to make a tile able to grow this kind of palm tree.
	/// </summary>
	public abstract class ModPalmTree
	{
		/// <summary>
		/// Return the type of dust created when this palm tree is destroyed. Returns 215 by default.
		/// </summary>
		/// <returns></returns>
		public virtual int CreateDust() {
			return 215;
		}

		/// <summary>
		/// Return the type of gore created to represent leaves when this palm tree grows on-screen. Returns -1 by default.
		/// </summary>
		/// <returns></returns>
		public virtual int GrowthFXGore() {
			return -1;
		}

		/// <summary>
		/// The ID of the item that is dropped in bulk when this palm tree is destroyed.
		/// </summary>
		/// <returns></returns>
		public abstract int DropWood();

		/// <summary>
		/// Return the texture that represents the tile sheet used for drawing this palm tree.
		/// </summary>
		/// <returns></returns>
		public abstract Texture2D GetTexture();

		/// <summary>
		/// Return the texture containing the possible tree tops that can be drawn above this palm tree.
		/// </summary>
		/// <returns></returns>
		public abstract Texture2D GetTopTextures();
	}

	/// <summary>
	/// This class represents a type of modded cactus. The cactus will share a tile ID with the vanilla cacti (80), so that the cacti can freely convert between each other if the sand below is converted. This class encapsulates a function for retrieving the cactus's texture, the only difference between each type of cactus. Use ModTile.SetModCactus or GlobalTile.AddModCactus to make a tile able to grow this kind of cactus.
	/// </summary>
	public abstract class ModCactus
	{
		/// <summary>
		/// Return the texture that represents the tile sheet used for drawing this cactus.
		/// </summary>
		/// <returns></returns>
		public abstract Texture2D GetTexture();
	}
}
