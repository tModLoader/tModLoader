namespace Terraria.ModLoader.Setup.Core;

public sealed record RoslynTaskParameters
{
	public required string ProjectPath { get; init; }
}