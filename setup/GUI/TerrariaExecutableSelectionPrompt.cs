using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.GUI;

public class TerrariaExecutableSelectionPrompt : ITerrariaExecutableSelectionPrompt
{
	private readonly ProgramSettings programSettings;

	public TerrariaExecutableSelectionPrompt(ProgramSettings programSettings)
	{
		this.programSettings = programSettings;
	}

	public Task<string> Prompt(CancellationToken cancellationToken = default)
	{
		var dialog = new OpenFileDialog {
			InitialDirectory = Path.GetFullPath(Directory.Exists(programSettings.TerrariaSteamDir) ? programSettings.TerrariaSteamDir : "."),
			Filter = "Terraria|Terraria.exe",
			Title = "Select Terraria.exe"
		};

		return Task.FromResult(dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileName);
	}
}