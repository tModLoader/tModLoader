using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class EncryptCommandSettings : CommandSettings
{
	private readonly string path;

	[CommandOption("-k|--key")]
	[Description("Key in hexadecimal")]
	public string? Key { get; init; }

	[CommandArgument(0, "<PATH>")]
	public required string Path {
		get => path;
		[MemberNotNull(nameof(path))]
		init => path = PathUtils.GetCrossPlatformFullPath(value);
	}
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

public sealed class OwnershipCommandSettings : CommandSettings
{
	private readonly string path;

	[CommandOption("-k|--key")]
	[Description("Key in hexadecimal")]
	public string? Key { get; init; }

	[CommandArgument(0, "<IDENTIFIER>")]
	public required string Identifier { get; init; }

	[CommandArgument(1, "<PATH>")]
	public required string Path {
		get => path;
		[MemberNotNull(nameof(path))]
		init => path = PathUtils.GetCrossPlatformFullPath(value);
	}
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

public sealed class RevealKeySettings : CommandSettings;

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