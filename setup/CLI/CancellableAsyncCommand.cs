using Spectre.Console.Cli;

namespace Terraria.ModLoader.Setup.CLI;

public abstract class CancellableAsyncCommand : AsyncCommand
{
	private readonly ConsoleAppCancellationTokenSource cancellationTokenSource = new();

	public abstract Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);

	public sealed override async Task<int> ExecuteAsync(CommandContext context)
		=> await ExecuteAsync(context, cancellationTokenSource.Token);

}

public abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
	where TSettings : CommandSettings
{
	private readonly ConsoleAppCancellationTokenSource cancellationTokenSource = new();

	public abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken);

	public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
		=> await ExecuteAsync(context, settings, cancellationTokenSource.Token);
}