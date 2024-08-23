namespace Terraria.ModLoader.Setup.CLI;

internal sealed class ConsoleAppCancellationTokenSource : IDisposable
{
	private readonly CancellationTokenSource cts = new();

	public ConsoleAppCancellationTokenSource()
	{
		Console.CancelKeyPress += OnCancelKeyPress;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		using CancellationTokenRegistration _ = cts.Token.Register(
			() => {
				AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
				Console.CancelKeyPress -= OnCancelKeyPress;
			}
		);
	}

	public CancellationToken Token => cts.Token;

	public void Dispose() => cts.Dispose();

	private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
	{
		e.Cancel = true;
		cts.Cancel();
	}

	private void OnProcessExit(object? sender, EventArgs e)
	{
		if (!cts.IsCancellationRequested) {
			cts.Cancel();
		}
	}
}