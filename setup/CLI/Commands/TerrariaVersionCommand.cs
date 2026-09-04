using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class TerrariaVersionCommandSettings : CommandSettings
{
	private readonly string path;

	[CommandArgument(0, "<PATH>")]
	public required string Path {
		get => path;
		[MemberNotNull(nameof(path))]
		init => path = PathUtils.GetCrossPlatformFullPath(value);
	}
}

public sealed class TerrariaVersionCommand : Command<TerrariaVersionCommandSettings>
{
	public override int Execute(CommandContext context, TerrariaVersionCommandSettings settings)
	{
		Console.WriteLine(AssemblyName.GetAssemblyName(settings.Path).Version);
		return 0;
	}
}
