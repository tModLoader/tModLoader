using Spectre.Console;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI;

public class CSharpProjectSelectionPrompt : ICSharpProjectSelectionPrompt
{
	public async Task<string?> Prompt(string? currentProjectPath, CancellationToken cancellationToken = default)
	{
		TextPrompt<string> textPrompt = new TextPrompt<string>("Enter .csproj path: ")
			.Validate(path => IsProjectFile(TrimQuotationMarks(path)) && File.Exists(PreparePath(path)),
				"File is not a .csproj file or does not exist.");

		if (!string.IsNullOrWhiteSpace(currentProjectPath)) {
			textPrompt.DefaultValue(currentProjectPath);
		}

		string path = await textPrompt.ShowAsync(AnsiConsole.Console, cancellationToken);

		return PreparePath(path);
	}

	private static string PreparePath(string path) => PathUtils.GetCrossPlatformFullPath(TrimQuotationMarks(path));

	private static string TrimQuotationMarks(string path) => path.Trim('"', '\'');

	private static bool IsProjectFile(string path) => Path.GetExtension(path).Equals(".csproj", StringComparison.OrdinalIgnoreCase);
}