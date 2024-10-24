namespace Terraria.ModLoader.Setup.Core;

public sealed record DiffTaskParameters
{
	public required string BaseDir { get; init; }

	public required string PatchedDir { get; init; }

	public required string PatchDir { get; init; }

	public required ProgramSetting<DateTime?> Cutoff { get; init; }

	public static DiffTaskParameters ForTerraria(ProgramSettings programSettings)
	{
		return new DiffTaskParameters {
			BaseDir = PathConstants.DecompiledFolder,
			PatchedDir = PathConstants.TerrariaSourceFolder,
			PatchDir = PathConstants.TerrariaPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaDiffCutoff, programSettings),
		};
	}

	public static DiffTaskParameters ForTerrariaNetCore(ProgramSettings programSettings)
	{
		return new DiffTaskParameters {
			BaseDir = PathConstants.TerrariaSourceFolder,
			PatchedDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchDir = PathConstants.TerrariaNetCorePatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaNetCoreDiffCutoff, programSettings),
		};
	}

	public static DiffTaskParameters ForTModLoader(ProgramSettings programSettings)
	{
		return new DiffTaskParameters {
			BaseDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchedDir = PathConstants.TModLoaderSourceFolder,
			PatchDir = PathConstants.TModLoaderPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TModLoaderDiffCutoff, programSettings),
		};
	}
}