namespace Terraria.ModLoader.Setup.Core;

public class SecretKeyProvider
{
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;
	private readonly TerrariaDecompileExecutableProvider terrariaDecompileExecutableProvider;

	private readonly Version PreviousKeyClientVersion = new("1.4.4.9");

	public SecretKeyProvider(TerrariaExecutableSetter terrariaExecutableSetter, TerrariaDecompileExecutableProvider terrariaDecompileExecutableProvider)
	{
		this.terrariaExecutableSetter = terrariaExecutableSetter;
		this.terrariaDecompileExecutableProvider = terrariaDecompileExecutableProvider;
	}

	public async Task<byte[]> DeriveKey(CancellationToken cancellationToken)
	{
		await terrariaExecutableSetter.FindAndSetTerrariaDirectoryIfNecessary(cancellationToken: cancellationToken);

		var prevVersionPath = terrariaDecompileExecutableProvider.GetVersionedExeBackupPath("Terraria", PreviousKeyClientVersion);
		if (File.Exists(prevVersionPath) && Secrets.TryDeriveKey(prevVersionPath, out var key))
			return key;

		return Secrets.DeriveKey(terrariaExecutableSetter.WorkspaceInfo.TerrariaPath);
	}
}