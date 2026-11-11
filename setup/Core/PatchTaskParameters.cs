namespace Terraria.ModLoader.Setup.Core;

public sealed record PatchTaskParameters
{
	/// <summary>Name of the layer, matching its patch and diff subcommands.</summary>
	public required string Name { get; init; }

	public required string BaseDir { get; init; }

	public required string PatchedDir { get; init; }

	public required string PatchDir { get; init; }

	public required ProgramSetting<DateTime?> Cutoff { get; init; }

	public static PatchTaskParameters[] All(ProgramSettings programSettings) => [
		ForTerraria(programSettings),
		ForTerrariaNetCore(programSettings),
		ForTModLoader(programSettings),
	];

	public static PatchTaskParameters ForTerraria(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			Name = "terraria",
			BaseDir = PathConstants.DecompiledFolder,
			PatchedDir = PathConstants.TerrariaSourceFolder,
			PatchDir = PathConstants.TerrariaPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaDiffCutoff, programSettings),
		};
	}

	public static PatchTaskParameters ForTerrariaNetCore(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			Name = "netcore",
			BaseDir = PathConstants.TerrariaSourceFolder,
			PatchedDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchDir = PathConstants.TerrariaNetCorePatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaNetCoreDiffCutoff, programSettings),
		};
	}

	public static PatchTaskParameters ForTModLoader(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			Name = "tml",
			BaseDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchedDir = PathConstants.TModLoaderSourceFolder,
			PatchDir = PathConstants.TModLoaderPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TModLoaderDiffCutoff, programSettings),
		};
	}
}