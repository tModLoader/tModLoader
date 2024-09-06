namespace Terraria.ModLoader.Setup.Core;

public sealed record DecompileTaskParameters
{
	public string? TerrariaSteamDirectory { get; set; }

	public string? TmlDevSteamDirectory { get; set; }

	public required string SrcDir { get; init; }

	public bool ServerOnly { get; init; }

	public int? MaxDegreeOfParallelism { get; init; }

	public static DecompileTaskParameters CreateDefault(
		string? terrariaSteamDirectory,
		string? tmlDevSteamDirectory,
		bool serverOnly = false,
		int? maxDegreeOfParallelism = null)
	{
		return new DecompileTaskParameters {
			TerrariaSteamDirectory = terrariaSteamDirectory,
			TmlDevSteamDirectory = tmlDevSteamDirectory,
			SrcDir = serverOnly ? "src/decompiled_server" : "src/decompiled",
			ServerOnly = serverOnly,
			MaxDegreeOfParallelism = maxDegreeOfParallelism,
		};
	}
}