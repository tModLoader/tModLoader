namespace Terraria.ModLoader.Setup.Core;

public static class TaskExtensions
{
	/// <summary>
	///		Throws an <see cref="AggregateException"/> instead of only throwing the first exception if the task faults
	///		with multiple exceptions. See also: https://github.com/dotnet/runtime/issues/47605#issuecomment-778930734
	/// </summary>
	/// <param name="source">The source task.</param>
	public static async Task WithAggregateException(this Task source)
	{
		try {
			await source.ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (source.IsCanceled) {
			throw;
		}
		catch {
			source.Wait();
		}
	}
}