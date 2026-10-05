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
	private readonly SecretKeyProvider keyProvider;

	public SecretEncryptCommand(SecretKeyProvider keyProvider)
	{
		this.keyProvider = keyProvider;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, EncryptCommandSettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = settings.Key != null ? Convert.FromHexString(settings.Key) : await keyProvider.DeriveKey(cancellationToken);
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
	private readonly SecretKeyProvider keyProvider;

	public SecretOwnershipCommand(SecretKeyProvider keyProvider)
	{
		this.keyProvider = keyProvider;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, OwnershipCommandSettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = settings.Key != null ? Convert.FromHexString(settings.Key) : await keyProvider.DeriveKey(cancellationToken);
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
	private readonly SecretKeyProvider keyProvider;

	public RevealKeyCommand(SecretKeyProvider keyProvider)
	{
		this.keyProvider = keyProvider;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, RevealKeySettings settings, CancellationToken cancellationToken)
	{
		try {
			var key = await keyProvider.DeriveKey(cancellationToken);
			Console.WriteLine(Convert.ToHexString(key).ToLower());
			return 0;
		}
		catch (Exception ex) {
			AnsiConsole.WriteException(ex);
			return 1;
		}
	}
}