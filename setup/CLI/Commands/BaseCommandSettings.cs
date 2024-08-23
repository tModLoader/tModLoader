using Spectre.Console.Cli;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public class BaseCommandSettings : CommandSettings
{
	[CommandOption("--plain-progress")]
	public bool PlainProgress { get; init; }
}