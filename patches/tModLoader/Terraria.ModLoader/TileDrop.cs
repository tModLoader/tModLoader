using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This struct is used in conjunction with GlobalTile.Drop() and ModTile.Drop(), see ExampleGlobalTile for usage.
	/// </summary>
	public struct TileDrop
	{
		/// <summary>
		/// Size of the tile, x being width and y being height (both in tilesize)
		/// </summary>
		public Vector2 TileSize
		{
			get
			{
				return new Vector2(TileWidth, TileHeight);
			}
			set
			{
				TileWidth = (int) value.X;
				TileHeight = (int) value.Y;
			}
		}

		/// <summary>
		/// Width of the tile in tile size (16 pixels = 1 tilewidth)
		/// </summary>
		public int TileWidth { get; set; }
		/// <summary>
		/// Height of the tile in tile size (16 pixels = 1 tileheight)
		/// </summary>
		public int TileHeight { get; set; }

		public static TileDrop Empty = default(TileDrop);
		// Known vanilla styles
		public static TileDrop Style1x1 = new TileDrop(1, 1);
		public static TileDrop Style1x2 = new TileDrop(1, 2);
		public static TileDrop Style1x3 = new TileDrop(1, 3);
		public static TileDrop Style1x5 = new TileDrop(1, 5);
		/// <summary>
		/// Height will be 0
		/// </summary>
		public static TileDrop Style1xX = new TileDrop(1, 0);
		public static TileDrop Style2x1 = new TileDrop(2, 1);
		public static TileDrop Style2x2 = new TileDrop(2, 2);
		public static TileDrop Style2x3 = new TileDrop(2, 3);
		public static TileDrop Style2x4 = new TileDrop(2, 4);
		/// <summary>
		/// Height will be 0
		/// </summary>
		public static TileDrop Style2xX = new TileDrop(2, 0);
		public static TileDrop Style3x1 = new TileDrop(3, 1);
		public static TileDrop Style3x2 = new TileDrop(3, 2);
		public static TileDrop Style3x3 = new TileDrop(3, 3);
		public static TileDrop Style3x4 = new TileDrop(3, 4);
		public static TileDrop Style4x2 = new TileDrop(4, 2);
		public static TileDrop Style4x3 = new TileDrop(4, 3);
		public static TileDrop Style4x8 = new TileDrop(4, 8);
		public static TileDrop Style5x4 = new TileDrop(5, 4);
		public static TileDrop Style6x2 = new TileDrop(6, 2);
		public static TileDrop Style6x3 = new TileDrop(6, 3);

		public TileDrop(int width, int height)
		{
			TileWidth = width;
			TileHeight = height;
		}

		public bool Equals(TileDrop other)
		{
			return Equals(other, this);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var other = (TileDrop)obj;
			return other.TileSize == this.TileSize;
		}

		public override int GetHashCode()
		{
			return (TileWidth + TileHeight).GetHashCode();
		}

		public static bool operator ==(TileDrop tD1, TileDrop tD2)
		{
			return tD1.Equals(tD2);
		}

		public static bool operator !=(TileDrop tD1, TileDrop tD2)
		{
			return !tD1.Equals(tD2);
		}

		public static TileDrop operator +(TileDrop tD1, TileDrop tD2)
		{
			return new TileDrop(tD1.TileWidth + tD2.TileWidth, tD1.TileHeight + tD2.TileHeight);
		}

		public static TileDrop operator -(TileDrop tD1, TileDrop tD2)
		{
			return new TileDrop(tD1.TileWidth - tD2.TileWidth, tD1.TileHeight - tD2.TileHeight);
		}

		public static TileDrop operator %(TileDrop tD1, TileDrop tD2)
		{
			return new TileDrop(tD1.TileWidth % tD2.TileWidth, tD1.TileHeight % tD2.TileHeight);
		}

		public static TileDrop operator *(TileDrop tD1, TileDrop tD2)
		{
			return new TileDrop(tD1.TileWidth * tD2.TileWidth, tD1.TileHeight * tD2.TileHeight);
		}

		public static TileDrop operator /(TileDrop tD1, TileDrop tD2)
		{
			return new TileDrop(tD1.TileWidth / tD2.TileWidth, tD1.TileHeight / tD2.TileHeight);
		}
	}
}
