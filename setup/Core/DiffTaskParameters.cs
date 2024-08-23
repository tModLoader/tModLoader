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
			BaseDir = "src/decompiled",
			PatchedDir = "src/Terraria",
			PatchDir = "patches/Terraria",
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaDiffCutoff, programSettings),
		};
	}

	public static DiffTaskParameters ForTerrariaNetCore(ProgramSettings programSettings)
	{
		return new DiffTaskParameters {
			BaseDir = "src/Terraria",
			PatchedDir = "src/TerrariaNetCore",
			PatchDir = "patches/TerrariaNetCore",
			Cutoff = new ProgramSetting<DateTime?>(x => x.TerrariaNetCoreDiffCutoff, programSettings),
		};
	}

	public static DiffTaskParameters ForTModLoader(ProgramSettings programSettings)
	{
		return new DiffTaskParameters {
			BaseDir = "src/TerrariaNetCore",
			PatchedDir = "src/tModLoader",
			PatchDir = "patches/tModLoader",
			Cutoff = new ProgramSetting<DateTime?>(x => x.TModLoaderDiffCutoff, programSettings),
		};
	}
}