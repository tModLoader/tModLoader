using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

/// <summary>
/// Tree statistics
/// </summary>
public struct TreeStats
{
	/// <summary>
	/// Total tree blocks
	/// </summary>
	public int TotalBlocks = 0;

	/// <summary>
	/// Total tree branches
	/// </summary>
	public int TotalBranches = 0;

	/// <summary>
	/// Total leafy branches
	/// </summary>
	public int LeafyBranches = 0;

	/// <summary>
	/// True if tree top exisis and not broken
	/// </summary>
	public bool HasTop = false;

	/// <summary>
	/// True if tree top exisis and broken
	/// </summary>
	public bool BrokenTop = false;

	/// <summary>
	/// True if thee have left root
	/// </summary>
	public bool LeftRoot = false;

	/// <summary>
	/// True if thee have right root
	/// </summary>
	public bool RightRoot = false;

	/// <summary>
	/// Tree top position
	/// </summary>
	public Point Top = new(0, int.MaxValue);

	/// <summary>
	/// Tree bottom position
	/// </summary>
	public Point Bottom = new(0, 0);

	/// <summary>
	/// Tree ground tile type
	/// </summary>
	public ushort GroundType = 0;

	public TreeStats()
	{
	}

	/// <summary/>
	public override string ToString()
	{
		return $"T:{TotalBlocks} " +
			$"B:{TotalBranches} " +
			$"BL:{LeafyBranches} " +
			$"{(HasTop ? BrokenTop ? "TB " : "TL " : "")}" +
			$"{(LeftRoot ? "Rl " : "")}" +
			$"{(RightRoot ? "Rr " : "")}" +
			$"X:{Top.X} Y:{Top.Y}-{Bottom.Y} G:{GroundType}";
	}
}
