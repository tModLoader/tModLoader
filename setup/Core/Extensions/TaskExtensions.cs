namespace Terraria.ModLoader.Setup.Core;

public static class TaskExtensions
{
	/// <summary>
	///		Throws an <see cref="AggregateException"/> if the task faults instead of only throwing the first exception.
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