using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class ProjectPathCommandSettings : BaseCommandSettings
{
	private string projectPath;

	[CommandArgument(0, "<PROJECT_PATH>")]
	[Description("Path to the .csproj file.")]
	public required string ProjectPath
	{
		get => projectPath;
		[MemberNotNull(nameof(projectPath))]
		set => projectPath = PathUtils.GetCrossPlatformFullPath(value);
	}

	public override ValidationResult Validate()
	{
		if (Path.GetExtension(projectPath) != ".csproj")
			return ValidationResult.Error("The project path must point to a .csproj file");

		if (!File.Exists(projectPath))
			return ValidationResult.Error("The project path does not exist");

		return ValidationResult.Success();
	}
}