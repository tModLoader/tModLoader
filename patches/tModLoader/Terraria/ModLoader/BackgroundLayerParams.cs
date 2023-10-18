namespace Terraria.ModLoader;

public unsafe readonly ref struct BackgroundLayerParams
{
	internal float* ScalePtr { get; init; }
	internal double* ParallaxPtr { get; init; }
	internal int* TopYPtr { get; init; }
	internal int* LoopWidthPtr { get; init; }

	/// <summary>
	/// The scale.
	/// </summary>
	public ref float Scale => ref *ScalePtr;

	/// <summary>
	/// The parallax value.
	/// </summary>
	public ref double Parallax => ref *ParallaxPtr;

	/// <summary>
	/// The Top Y level that the BG is drawn at
	/// </summary>
	public ref int TopY => ref *TopYPtr;

	/// <summary>
	/// The looping width that the texture loops at. Defaults to (Main.screenWidth / Main.bgWidthScaled + 2)
	/// </summary>
	public ref int LoopWidth => ref *LoopWidthPtr;
}
