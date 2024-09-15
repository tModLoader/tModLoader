using Spectre.Console;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.CLI;

public class UserPrompt : IUserPrompt
{
	private const string Cancel = "Cancel";
	private const string OK = "OK";
	private const string Retry = "Retry";

	public Task<bool> Prompt(
		string caption,
		string text,
		PromptOptions options,
		PromptSeverity severity = PromptSeverity.Information)
	{
		Inform(caption, text, severity);

		string answer = AnsiConsole.Prompt(
			new SelectionPrompt<string>().AddChoices(options == PromptOptions.OKCancel ? OK : Retry, Cancel));

		if (answer != Cancel) {
			return Task.FromResult(true);
		}

		Console.WriteLine("Cancelled");

		return Task.FromResult(false);
	}

	public Task Inform(string caption, string text, PromptSeverity severity = PromptSeverity.Information)
	{
		AnsiConsole.MarkupLineInterpolated($"[{GetColor(severity).ToString().ToLower()}]{severity}[/] {caption}");
		AnsiConsole.WriteLine(text);
		AnsiConsole.WriteLine();

		return Task.CompletedTask;
	}

	private static Color GetColor(PromptSeverity severity)
	{
		return severity switch {
			PromptSeverity.Information => Color.Blue,
			PromptSeverity.Warning => Color.Yellow,
			PromptSeverity.Error => Color.Red,
			_ => Color.White,
		};
	}
}