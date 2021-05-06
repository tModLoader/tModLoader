using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This enum dictates were in the vanilla mining tool code chain will a ToolType's bahavior take place. 
	/// Setting the Priority property of a ToolType to an appropriate ToolPriority will allow restricting when a ToolType's behavior will run.
	/// ToolTypes with higher priority can stop other ToolTypes down the chain from executing their behavior. 
	/// If multiple ToolTypes which share the same priority are added to a tool, Vanilla ones will run first, then Modded ones based on order in which they were loaded.
	/// </summary>
	public enum ToolPriority
	{
		/// <summary> Runs before any other ToolTypes on a tool, including Vanilla ones. </summary>
		First,
		/// <summary> Runs only for blocks, after the shovel, and before all other Vanilla ToolTypes. </summary>
		PreMineBlock,
		/// <summary> Runs after the Vanilla block mining code, if all other ToolTypes that can mine blocks didn't met their usage conditions. Check the ToolType docs for more info. </summary>
		MineBlock,
		/// <summary> Runs after the block mining code and before the hammer's slope/half-block code. </summary>
		PreSlopeBlock,
		/// <summary> Runs after the hammer's slope/half-block code, if it didn't met its usage condition. </summary>
		SlopeBlock,
		/// <summary> Runs after the block mining and hammer's slope/half-block code, and before the hammer's wall mining code, if it allowed to run. </summary>
		PreHitWall,
		/// <summary> Runs after the hammer's wall mining code. </summary>
		HitWall,
		/// <summary> Runs after all other ToolTypes, if the usage conditions of all the ones before it weren't met. </summary>
		Last
	}
}