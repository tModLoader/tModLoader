using System.ComponentModel;
using Spectre.Console.Cli;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public class BaseCommandSettings : CommandSettings
{
	[CommandOption("--plain-progress")]
	public bool PlainProgress { get; init; }

	[CommandOption("--strict")]
	[Description("Exit with non-success instead of success exit code on warnings.")]
	public bool Strict { get; init; }
}