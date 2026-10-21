using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;

namespace Terraria;

public partial class NetMessage
{
	/// <summary>
	/// <inheritdoc cref="SendTileSquare(int, int, int, int, TileChangeType)" path="/SharedSummary/node()"/>
	/// <br/><br/> In this particular overload, <paramref name="liquidType"/> and <paramref name="otherLiquid"/> are used to send a liquid merge sound. Specifically used by Terraria.Liquid.LiquidCheck. <paramref name="liquidType"/> and <paramref name="otherLiquid"/> are the two liquid IDs merging together.
	/// </summary>
	public static void SendTileSquare(int whoAmi, int tileX, int tileY, int centeredSquareSize, byte liquidType, byte otherLiquid)
	{
		int num = (centeredSquareSize - 1) / 2;
		SendTileSquare(whoAmi, tileX - num, tileY - num, centeredSquareSize, centeredSquareSize, liquidType, otherLiquid);
	}

	/// <summary>
	/// <inheritdoc cref="SendTileSquare(int, int, int, int, int, TileChangeType)" path="/SharedSummary/node()"/>
	/// In this particular overload, <paramref name="liquidType"/> and <paramref name="otherLiquid"/> are used to send a liquid merge sound. Specifically used by Terraria.Liquid.LiquidCheck. <paramref name="liquidType"/> and <paramref name="otherLiquid"/> are the two liquid IDs merging together.
	/// </summary>
	public static void SendTileSquare(int whoAmi, int tileX, int tileY, int xSize, int ySize, byte liquidType, byte otherLiquid)
	{
		SendData(20, whoAmi, -1, null, tileX, tileY, xSize, ySize, liquidType, otherLiquid + 1);
		WorldGen.RangeFrame(tileX, tileY, xSize, ySize);
	}
}
