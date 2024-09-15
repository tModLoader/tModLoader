using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Setup.GUI.Avalonia.Services;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Setup.GUI.Avalonia;

public class TerrariaExecutableSelectionPrompt : ITerrariaExecutableSelectionPrompt
{
	private readonly IFilesService filesService;
	private readonly WorkspaceInfo workspaceInfo;

	public TerrariaExecutableSelectionPrompt(IFilesService filesService, WorkspaceInfo workspaceInfo)
	{
		this.filesService = filesService;
		this.workspaceInfo = workspaceInfo;
	}

	public async Task<string> Prompt(CancellationToken cancellationToken = default)
	{
		IStorageFile? storageFile = await filesService.OpenFile(
			"Select Terraria.exe",
			[new FilePickerFileType("Terraria") { Patterns = ["Terraria.exe"] }],
			Path.GetFullPath(Directory.Exists(workspaceInfo.TerrariaSteamDirectory) ? workspaceInfo.TerrariaSteamDirectory : "."));

		string? path = storageFile?.TryGetLocalPath();

		if (path is null) {
			throw new OperationCanceledException();
		}

		return path;
	}
}