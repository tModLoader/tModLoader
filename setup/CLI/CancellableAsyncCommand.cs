using Spectre.Console.Cli;

namespace Terraria.ModLoader.Setup.CLI;

public abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
	where TSettings : CommandSettings
{
	private readonly ConsoleAppCancellationTokenSource cancellationTokenSource = new();

	public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
		=> await ExecuteAsync(context, settings, cancellationTokenSource.Token);

	protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken);
}