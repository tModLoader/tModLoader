using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.GUI;

public class TerrariaExecutableSelectionPrompt : ITerrariaExecutableSelectionPrompt
{
	private readonly WorkspaceInfo workspaceInfo;

	public TerrariaExecutableSelectionPrompt(WorkspaceInfo workspaceInfo)
	{
		this.workspaceInfo = workspaceInfo;
	}

	public Task<string> Prompt(CancellationToken cancellationToken = default)
	{
		var dialog = new OpenFileDialog {
			InitialDirectory = Path.GetFullPath(Directory.Exists(workspaceInfo.TerrariaSteamDirectory) ? workspaceInfo.TerrariaSteamDirectory : "."),
			Filter = "Terraria|Terraria.exe",
			Title = "Select Terraria.exe"
		};

		var result = dialog.ShowDialog();
		if (result == DialogResult.OK)
			return Task.FromResult(dialog.FileName);

		throw new OperationCanceledException();
	}
}