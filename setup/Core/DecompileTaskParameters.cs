namespace Terraria.ModLoader.Setup.Core;

public sealed record DecompileTaskParameters
{
	public required string SrcDir { get; init; }

	public bool ServerOnly { get; init; }

	public int? MaxDegreeOfParallelism { get; init; }

	public static DecompileTaskParameters CreateDefault(bool serverOnly = false, int? maxDegreeOfParallelism = null)
	{
		return new DecompileTaskParameters {
			SrcDir = serverOnly ? "src/decompiled_server" : "src/decompiled",
			ServerOnly = serverOnly,
			MaxDegreeOfParallelism = maxDegreeOfParallelism,
		};
	}
}