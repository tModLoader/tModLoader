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
	public struct TileStyle
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
				TileWidth = (int)value.X;
				TileHeight = (int)value.Y;
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

		public static TileStyle Empty = default(TileStyle);
		// Known vanilla styles
		public static TileStyle Style1x1 = new TileStyle(1, 1);
		public static TileStyle Style1x2 = new TileStyle(1, 2);
		public static TileStyle Style1x3 = new TileStyle(1, 3);
		public static TileStyle Style1x5 = new TileStyle(1, 5);
		public static TileStyle Style1xX = new TileStyle(1, 0);
		public static TileStyle Style2x1 = new TileStyle(2, 1);
		public static TileStyle Style2x2 = new TileStyle(2, 2);
		public static TileStyle Style2x3 = new TileStyle(2, 3);
		public static TileStyle Style2x4 = new TileStyle(2, 4);
		public static TileStyle Style2xX = new TileStyle(2, 0);
		public static TileStyle Style3x1 = new TileStyle(3, 1);
		public static TileStyle Style3x2 = new TileStyle(3, 2);
		public static TileStyle Style3x3 = new TileStyle(3, 3);
		public static TileStyle Style3x4 = new TileStyle(3, 4);
		public static TileStyle Style4x2 = new TileStyle(4, 2);
		public static TileStyle Style4x3 = new TileStyle(4, 3);
		public static TileStyle Style4x8 = new TileStyle(4, 8);
		public static TileStyle Style5x4 = new TileStyle(5, 4);
		public static TileStyle Style6x2 = new TileStyle(6, 2);
		public static TileStyle Style6x3 = new TileStyle(6, 3);

		public TileStyle(int width, int height)
		{
			TileWidth = width;
			TileHeight = height;
		}

		public bool Equals(TileStyle other)
		{
			return Equals(other, this);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var other = (TileStyle)obj;
			return other.TileSize == this.TileSize;
		}

		public override int GetHashCode()
		{
			return (TileWidth + TileHeight).GetHashCode();
		}

		public static bool operator ==(TileStyle tD1, TileStyle tD2)
		{
			return tD1.Equals(tD2);
		}

		public static bool operator !=(TileStyle tD1, TileStyle tD2)
		{
			return !tD1.Equals(tD2);
		}

		public static TileStyle operator +(TileStyle tD1, TileStyle tD2)
		{
			return new TileStyle(tD1.TileWidth + tD2.TileWidth, tD1.TileHeight + tD2.TileHeight);
		}

		public static TileStyle operator -(TileStyle tD1, TileStyle tD2)
		{
			return new TileStyle(tD1.TileWidth - tD2.TileWidth, tD1.TileHeight - tD2.TileHeight);
		}

		public static TileStyle operator %(TileStyle tD1, TileStyle tD2)
		{
			return new TileStyle(tD1.TileWidth % tD2.TileWidth, tD1.TileHeight % tD2.TileHeight);
		}

		public static TileStyle operator *(TileStyle tD1, TileStyle tD2)
		{
			return new TileStyle(tD1.TileWidth * tD2.TileWidth, tD1.TileHeight * tD2.TileHeight);
		}

		public static TileStyle operator /(TileStyle tD1, TileStyle tD2)
		{
			return new TileStyle(tD1.TileWidth / tD2.TileWidth, tD1.TileHeight / tD2.TileHeight);
		}
	}
}
