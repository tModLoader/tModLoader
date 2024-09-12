namespace Terraria.ModLoader.Setup.Core;

public sealed record PatchTaskParameters
{
	public required string BaseDir { get; init; }

	public required string PatchedDir { get; init; }

	public required string PatchDir { get; init; }

	public required ProgramSetting<DateTime?> Cutoff { get; init; }

	public static PatchTaskParameters ForTerraria(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			BaseDir = PathConstants.DecompiledFolder,
			PatchedDir = PathConstants.TerrariaSourceFolder,
			PatchDir = PathConstants.TerrariaPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaDiffCutoff, programSettings),
		};
	}

	public static PatchTaskParameters ForTerrariaNetCore(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			BaseDir = PathConstants.TerrariaSourceFolder,
			PatchedDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchDir = PathConstants.TerrariaNetCorePatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaNetCoreDiffCutoff, programSettings),
		};
	}

	public static PatchTaskParameters ForTModLoader(ProgramSettings programSettings)
	{
		return new PatchTaskParameters {
			BaseDir = PathConstants.TerrariaNetCoreSourceFolder,
			PatchedDir = PathConstants.TModLoaderSourceFolder,
			PatchDir = PathConstants.TModLoaderPatchesFolder,
			Cutoff = new ProgramSetting<DateTime?>(x => x.TModLoaderDiffCutoff, programSettings),
		};
	}
}