using Spectre.Console;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class TerrariaExecutableSelectionPrompt : ITerrariaExecutableSelectionPrompt
{
	public async Task<string> Prompt(CancellationToken cancellationToken = default)
	{
		TextPrompt<string> textPrompt = new TextPrompt<string>("Enter Terraria.exe path: ")
			.Validate(path => IsTerrariaExe(TrimQuotationMarks(path)) && File.Exists(PreparePath(path)), "File is not Terraria.exe or does not exist.");

		string path = await textPrompt.ShowAsync(AnsiConsole.Console, cancellationToken);

		return PreparePath(path);
	}

	private static string PreparePath(string path) => PathUtils.GetCrossPlatformFullPath(TrimQuotationMarks(path));

	private static string TrimQuotationMarks(string path) => path.Trim('"', '\'');

	private static bool IsTerrariaExe(string path) => Path.GetFileName(path).Equals("Terraria.exe", StringComparison.OrdinalIgnoreCase);

}