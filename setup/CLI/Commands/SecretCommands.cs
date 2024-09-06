using System;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class EncryptCommandSettings : BaseCommandSettings
{
	[CommandOption("--key")]
	[Description("Key in hexadecimal")]
	public string Key { get; init; }

	[CommandArgument(0, "<path>")]
	public string Path { get; init; }
}

public sealed class SecretEncryptCommand : CancellableAsyncCommand<EncryptCommandSettings>
{
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;

	public SecretEncryptCommand(TerrariaExecutableSetter terrariaExecutableSetter)
	{
		this.terrariaExecutableSetter = terrariaExecutableSetter;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, EncryptCommandSettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = settings.Key != null ? Convert.FromHexString(settings.Key) : Secrets.DeriveKey(await terrariaExecutableSetter.CheckTerrariaExecutablePathsAndPromptIfNecessary(cancellationToken));
			new Secrets(key).UpdateFile(settings.Path);
			return 0;
		}
		catch (Exception ex) {
			AnsiConsole.WriteException(ex);
			return 1;
		}
	}
}

public sealed class OwnershipCommandSettings : BaseCommandSettings
{
	[CommandOption("--key")]
	[Description("Key in hexadecimal")]
	public string Key { get; init; }

	[CommandArgument(0, "<name>")]
	public string Identifier { get; init; }

	[CommandArgument(1, "<path>")]
	public string Path { get; init; }
}

public sealed class SecretOwnershipCommand : CancellableAsyncCommand<OwnershipCommandSettings>
{
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;

	public SecretOwnershipCommand(TerrariaExecutableSetter terrariaExecutableSetter)
	{
		this.terrariaExecutableSetter = terrariaExecutableSetter;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, OwnershipCommandSettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = settings.Key != null ? Convert.FromHexString(settings.Key) : Secrets.DeriveKey(await terrariaExecutableSetter.CheckTerrariaExecutablePathsAndPromptIfNecessary(cancellationToken));
			new Secrets(key).AddProofOfOwnershipFile(settings.Identifier, settings.Path);
			return 0;
		}
		catch (Exception ex) {
			AnsiConsole.WriteException(ex);
			return 1;
		}
	}
}
public sealed class RevealKeySettings : BaseCommandSettings
{
}

public sealed class RevealKeyCommand : CancellableAsyncCommand<RevealKeySettings>
{
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;

	public RevealKeyCommand(TerrariaExecutableSetter terrariaExecutableSetter)
	{
		this.terrariaExecutableSetter = terrariaExecutableSetter;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, RevealKeySettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = Secrets.DeriveKey(await terrariaExecutableSetter.CheckTerrariaExecutablePathsAndPromptIfNecessary(cancellationToken));
			Console.WriteLine(Convert.ToHexString(key).ToLower());
			return 0;
		}
		catch (Exception ex) {
			AnsiConsole.WriteException(ex);
			return 1;
		}
	}
}