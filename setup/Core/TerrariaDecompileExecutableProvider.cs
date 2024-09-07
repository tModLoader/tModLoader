using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

internal sealed class TerrariaDecompileExecutableProvider
{
	private static readonly Version ClientVersion = new("1.4.4.9");
	private static readonly Version ServerVersion = new("1.4.4.9");

	private readonly WorkspaceInfo workspaceInfo;
	private readonly HttpClient httpClient;

	public TerrariaDecompileExecutableProvider(WorkspaceInfo workspaceInfo)
	{
		this.workspaceInfo = workspaceInfo;
		httpClient = new HttpClient();
	}

	private delegate Task FinalRetrievalAction(string destinationFileName);

	public async Task<string> RetrieveClientExecutable(CancellationToken cancellationToken = default)
	{
		return await Retrieve(
			"Terraria",
			ClientVersion,
			DecryptTerrariaExe);

		async Task DecryptTerrariaExe(string destinationPath)
		{
			CheckVersion(workspaceInfo.TerrariaPath, ClientVersion);

			if (!Secrets.TryDeriveKey(workspaceInfo.TerrariaPath, out var key)) {
				throw new InvalidOperationException($"Failed to derive key from '{workspaceInfo.TerrariaPath}'. Cannot decrypt Terraria Windows executable.");
			}

			byte[] decryptedFile = new Secrets(key).ReadFile(Path.GetFileName(destinationPath));
			await File.WriteAllBytesAsync(destinationPath, decryptedFile, cancellationToken);
		}
	}

	public async Task<string> RetrieveServerExecutable(ITaskProgress taskProgress, CancellationToken cancellationToken = default)
	{
		return await Retrieve(
			"TerrariaServer",
			ServerVersion,
			async destinationPath => {
				taskProgress.ReportStatus("Downloading TerrariaServer Windows executable...");
				await DownloadAndExtractTerrariaServer(destinationPath, cancellationToken);
			});
	}

	private async Task DownloadAndExtractTerrariaServer(string destinationPath, CancellationToken cancellationToken)
	{
		string serverVersionWithoutDots = ServerVersion.ToString().Replace(".", "");
		string url = $"https://terraria.org/api/download/pc-dedicated-server/terraria-server-{serverVersionWithoutDots}.zip";
		Stream fileStream = await httpClient.GetStreamAsync(url, cancellationToken);
		string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		ZipFile.ExtractToDirectory(fileStream, tempDirectory);

		File.Copy(Path.Combine(tempDirectory, serverVersionWithoutDots, "Windows", "TerrariaServer.exe"), destinationPath);
		Directory.Delete(tempDirectory, true);
	}

	private async Task<string> Retrieve(string fileNameWithoutExtension, Version version, FinalRetrievalAction finalRetrievalAction)
	{
		string expectedExeName = $"{fileNameWithoutExtension}_v{version}_win.exe";
		string expectedExePath = Path.Combine(workspaceInfo.TerrariaSteamDirectory, expectedExeName);
		string originalExePath = Path.Combine(workspaceInfo.TerrariaSteamDirectory, $"{fileNameWithoutExtension}.exe");
		if (File.Exists(expectedExePath)) {
			return expectedExePath;
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			if (File.Exists(Path.Combine(workspaceInfo.TerrariaSteamDirectory, $"{fileNameWithoutExtension}_v{version}.exe"))) {
				File.Move(Path.Combine(workspaceInfo.TerrariaSteamDirectory, $"{fileNameWithoutExtension}_v{version}.exe"), expectedExePath);
			}
			else {
				CheckVersion(originalExePath, version);
				File.Copy(originalExePath, expectedExePath);
			}

			return expectedExePath;
		}

		await finalRetrievalAction(expectedExePath);

		return expectedExePath;
	}

	private static void CheckVersion(string filePath, Version expectedVersion)
	{
		AssemblyName assemblyName = AssemblyName.GetAssemblyName(filePath);
		if (assemblyName.Version != expectedVersion) {
			throw new InvalidOperationException(
				$"{Path.GetFileName(filePath)} has unsupported version {assemblyName.Version}. Version {expectedVersion} was expected.");
		}
	}
}