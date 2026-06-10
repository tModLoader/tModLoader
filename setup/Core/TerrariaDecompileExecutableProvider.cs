using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using ICSharpCode.Decompiler.Metadata;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

public sealed class TerrariaDecompileExecutableProvider
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

	private delegate Task FallbackRetrievalMethod(string destinationFileName);

	public async Task<string> RetrieveClientExecutable(
		byte[]? key,
		CancellationToken cancellationToken = default)
	{
		return await Retrieve(
			"Terraria",
			ClientVersion,
			DecryptTerrariaExe);

		async Task DecryptTerrariaExe(string destinationPath)
		{
			if (key == null && !Secrets.TryDeriveKey(workspaceInfo.TerrariaPath, out key))
				throw new InvalidOperationException(
					$"Failed to derive key from '{workspaceInfo.TerrariaPath}'. Cannot decrypt Terraria Windows executable.");

			byte[] decryptedFile = new Secrets(key).ReadFile(Path.GetFileName(destinationPath));
			Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
			await File.WriteAllBytesAsync(destinationPath, decryptedFile, cancellationToken);

			CheckVersion(destinationPath, ClientVersion);
		}
	}

	public async Task<string> RetrieveServerExecutable(ITaskProgress taskProgress, CancellationToken cancellationToken = default)
	{
		return await Retrieve(
			"TerrariaServer",
			ServerVersion,
			DownloadAndExtractTerrariaServer);

		async Task DownloadAndExtractTerrariaServer(string destinationPath)
		{
			taskProgress.ReportStatus("Downloading TerrariaServer Windows executable...");

			string serverVersionWithoutDots = ServerVersion.ToString().Replace(".", "");
			string url = $"https://terraria.org/api/download/pc-dedicated-server/terraria-server-{serverVersionWithoutDots}.zip";
			using var zip = new ZipArchive(await httpClient.GetStreamAsync(url, cancellationToken));
			zip.Entries.Single(e => e.FullName == $"{serverVersionWithoutDots}/Windows/TerrariaServer.exe").ExtractToFile(destinationPath);
		}
	}

	public string GetVersionedExeBackupPath(string fileNameWithoutExtension, Version version)
	{
		string expectedExeName = $"{fileNameWithoutExtension}_v{version}_win.exe";
		return Path.Combine(workspaceInfo.TerrariaSteamDirectory, $"{fileNameWithoutExtension}_v{version}_win.exe");
	}

	private async Task<string> Retrieve(string fileNameWithoutExtension, Version version, FallbackRetrievalMethod fallbackRetrievalMethod)
	{
		string expectedExePath = GetVersionedExeBackupPath(fileNameWithoutExtension, version);
		string originalExePath = Path.Combine(workspaceInfo.TerrariaSteamDirectory, $"{fileNameWithoutExtension}.exe");
		if (File.Exists(expectedExePath)) {
			return expectedExePath;
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(originalExePath) && TryCheckVersion(originalExePath, version, out _)) {
			File.Copy(originalExePath, expectedExePath);
			return expectedExePath;
		}

		await fallbackRetrievalMethod(expectedExePath);
		return expectedExePath;
	}

	public static bool TryCheckVersion(string filePath, Version expectedVersion, out Version? actualVersion)
	{
		actualVersion = AssemblyName.GetAssemblyName(filePath).Version;
		return actualVersion == expectedVersion;
	}

	private static void CheckVersion(string filePath, Version expectedVersion)
	{
		if (!TryCheckVersion(filePath, expectedVersion, out var actualVersion))
			throw new InvalidOperationException(
				$"{Path.GetFileName(filePath)} has unsupported version {actualVersion}. Version {expectedVersion} was expected.");
	}

	public async Task<IReadOnlyCollection<string>> RetrieveExtraReferences(ITaskProgress taskProgress, CancellationToken cancellationToken = default)
	{
		var paths = new List<string>();
		if (File.Exists(Path.Combine(workspaceInfo.TerrariaSteamDirectory, "mscorlib.dll")))
			paths.Add(await RetrieveFrameworkRefs(taskProgress, cancellationToken));

		if (File.Exists(Path.Combine(workspaceInfo.TerrariaSteamDirectory, "FNA.dll"))
			|| UniversalAssemblyResolver.GetAssemblyInGac(AssemblyNameReference.Parse("Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553")) is null)
			paths.Add(Path.Combine("setup", "xna_redist"));

		paths.Add("patches/Terraria/Terraria/Libraries/Common"); // For SteelSeriesEngineWrapper.dll which was accidentally not shipped as an embedded ref in 1.4.4, and embeded in 1.4.5

		return paths;
	}

	private async Task<string> RetrieveFrameworkRefs(ITaskProgress taskProgress, CancellationToken cancellationToken = default)
	{
		// Note that we use v4.8.1 even though Terraria was compiled against v4.0 because ILSpy locates the 4.8.1 assemblies on windows machines for decompilation, as they're installed as 'drop in replacements'
		// There are some very minor (but nonetheless problematic) decompile output differences due to the back-compatible API differences in 4.8.1
		var path = Path.Combine("setup", ".NETFramework", "v4.8.1");
		if (Directory.Exists(path))
			return path;

		taskProgress.ReportStatus("Downloading .NET Framework Reference Assemblies...");
		var url = "https://www.nuget.org/api/v2/package/Microsoft.NETFramework.ReferenceAssemblies.net481/1.0.3";
		using var zip = new ZipArchive(await httpClient.GetStreamAsync(url, cancellationToken));

		var subfolder = "build/.NETFramework/v4.8.1";
		foreach (var e in zip.Entries) {
			if (e.FullName.StartsWith(subfolder) && e.FullName.EndsWith(".dll")) {
				var filePath = Path.Combine(path, e.FullName.Substring(subfolder.Length + 1));
				SetupOperation.CreateParentDirectory(filePath);
				e.ExtractToFile(filePath);
			}
		}

		return path;
	}
}
