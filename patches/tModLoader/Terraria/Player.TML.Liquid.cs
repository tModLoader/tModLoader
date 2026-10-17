using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Player
{
	/// <summary> True if the player is within crafting range of a tile with water or a <see cref="TileID.Sets.CountsAsWaterForCrafting"/> tile. (adjWater stands for adjacent to water) <br/> <br/> Legacy code, use <see cref="adjLiquid"/> instead. </summary>
	public bool adjWaterSource {
		get => adjLiquid[LiquidID.Water];
		set => adjLiquid[LiquidID.Water] = value;
	}
	/// <summary> True if the player is within crafting range of a tile with honey or a <see cref="TileID.Sets.CountsAsHoneyForCrafting"/> tile. (adjHoney stands for adjacent to honey) <br/> <br/> Legacy code, use <see cref="adjLiquid"/> instead. </summary>
	public bool adjHoney {
		get => adjLiquid[LiquidID.Honey];
		set => adjLiquid[LiquidID.Honey] = value;
	}
	/// <summary> True if the player is within crafting range of a tile with shimmer or a <see cref="TileID.Sets.CountsAsShimmerForCrafting"/> tile. (adjShimmer stands for adjacent to shimmer) <br/> <br/> Legacy code, use <see cref="adjLiquid"/> instead. </summary>
	public bool adjShimmer {
		get => adjLiquid[LiquidID.Shimmer];
		set => adjLiquid[LiquidID.Shimmer] = value;
	}
	/// <summary> True if the player is within crafting range of a tile with lava or a <see cref="TileID.Sets.CountsAsLavaForCrafting"/> tile. (adjLava stands for adjacent to lava) <br/> <br/> Legacy code, use <see cref="adjLiquid"/> instead. </summary>
	public bool adjLava {
		get => adjLiquid[LiquidID.Lava];
		set => adjLiquid[LiquidID.Lava] = value;
	}
	/// <summary>
	/// Legacy code, use <see cref="oldAdjLiquid"/> instead.
	/// </summary>
	public bool oldAdjWater {
		get => oldAdjLiquid[LiquidID.Water];
		set => oldAdjLiquid[LiquidID.Water] = value;
	}
	/// <summary>
	/// Legacy code, use <see cref="oldAdjLiquid"/> instead.
	/// </summary>
	public bool oldAdjHoney {
		get => oldAdjLiquid[LiquidID.Honey];
		set => oldAdjLiquid[LiquidID.Honey] = value;
	}
	/// <summary>
	/// Legacy code, use <see cref="oldAdjLiquid"/> instead.
	/// </summary>
	public bool oldAdjLava {
		get => oldAdjLiquid[LiquidID.Lava];
		set => oldAdjLiquid[LiquidID.Lava] = value;
	}
	/// <summary>
	/// Legacy code, use <see cref="oldAdjLiquid"/> instead.
	/// </summary>
	public bool oldAdjShimmer {
		get => oldAdjLiquid[LiquidID.Shimmer];
		set => oldAdjLiquid[LiquidID.Shimmer] = value;
	}

	private bool[] _adjLiquid = new bool[LiquidLoader.LiquidCount]; // Added by TML.

	/// <summary>
	/// An array indexed by liquid type (<see cref="Tile.LiquidType"/>, <see cref="LiquidID"/>) indicating if the player is within crafting range of a tile with lava or a <see cref="TileID.Sets.CountsAsLiquidSource"/> tile. Note that this includes liquids acting like other liquids (<see cref="ModLiquid.AdjLiquids"/>, <see cref="Player.AdjTiles"/>), so the player might not actually be near specific liquids.
	/// </summary>
	public bool[] adjLiquid {
		get {
			if (_adjLiquid.Length != LiquidLoader.LiquidCount) {
				Array.Resize(ref _adjLiquid, LiquidLoader.LiquidCount);
			}
			return _adjLiquid;
		}
		set {
			_adjLiquid = value;
		}
	}

	private bool[] _oldAdjLiquid = new bool[LiquidLoader.LiquidCount]; // Added by TML.

	public bool[] oldAdjLiquid {
		get {
			if (_oldAdjLiquid.Length != LiquidLoader.LiquidCount) {
				Array.Resize(ref _oldAdjLiquid, LiquidLoader.LiquidCount);
			}
			return _oldAdjLiquid;
		}
		set {
			_oldAdjLiquid = value;
		}
	}

	public void ModLiquidCollision(int liquidID, bool fallThrough, bool ignorePlats)
	{
		int num = ((!onTrack) ? height : (height - 20));
		Vector2 vector = velocity;
		velocity = Collision.TileCollision(position, velocity, width, num, fallThrough, ignorePlats, (int)gravDir);
		Vector2 vector2 = velocity * LiquidLoader.GetLiquid(liquidID).EntityMovementMultiplier;
		if (velocity.X != vector.X)
			vector2.X = velocity.X;

		if (velocity.Y != vector.Y)
			vector2.Y = velocity.Y;

		position += vector2;
		TryFloatingInFluid();
	}
}
