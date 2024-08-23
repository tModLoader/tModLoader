using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.GUI;

public class CSharpProjectSelectionPrompt : ICSharpProjectSelectionPrompt
{
	public Task<string> Prompt(string currentProjectPath, CancellationToken cancellationToken = default)
	{
		var dialog = new OpenFileDialog {
			FileName = currentProjectPath,
			InitialDirectory = Path.GetDirectoryName(currentProjectPath) ?? Path.GetFullPath("."),
			Filter = "C# Project|*.csproj",
			Title = "Select C# Project"
		};

		var result = dialog.ShowDialog();

		return Task.FromResult(result == DialogResult.OK ? dialog.FileName : null);
	}
}